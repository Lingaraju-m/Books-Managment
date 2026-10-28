using API_Practise.Data;
using API_Practise.Entities;
using API_Practise.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using QRCoder;

namespace API_Practise.Service
{
    public class AuthService(APIContext context, IConfiguration configuration) : IAuthService
    {


        public async Task<LoginChallengeDto> LoginAsync(userDto request)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null ||
                new PasswordHasher<User>().VerifyHashedPassword(
                    user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return new LoginChallengeDto
                {
                    RequiresOtp = false,
                    Message = "Invalid username or password."
                };
            }

            if (!user.IsTotpEnabled ||
                string.IsNullOrWhiteSpace(user.TotpSecretKey))
            {
                return new LoginChallengeDto
                {
                    RequiresOtp = false,
                    ChallengeToken = CreateLoginChallenge(
                        user, "enrollment"),
                    Message = "TOTP enrollment is required."
                };
            }

            return new LoginChallengeDto
            {
                RequiresOtp = true,
                ChallengeToken = CreateLoginChallenge(
                    user, "login_challenge"),
                Message = "Password verified. Enter your authenticator code."
            };
        }


        public async Task<User> RegisterAync(userDto request)
        {

            if (await context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null;
            }

            var user = new User();

            var hasedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            if (request.Role == "Admin")
            {
                user.Roles = "Admin";
            }
            else
            {
                user.Roles = "User";
            }

            user.Username = request.Username;
            user.PasswordHash = hasedPassword;

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return user;
        }


        public async Task<string?> GenerateTotpQrCodeAsync(int userId)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null || user.IsTotpEnabled)
                return null;

            if (string.IsNullOrWhiteSpace(user.TotpSecretKey))
            {
                var secret = KeyGeneration.GenerateRandomKey(20);
                user.TotpSecretKey = Base32Encoding.ToString(secret);

                await context.SaveChangesAsync();
            }

            var issuer = "MyBookApi";
            var account = Uri.EscapeDataString(user.Username);
            var encodedIssuer = Uri.EscapeDataString(issuer);

            var otpAuthUrl =
                $"otpauth://totp/{encodedIssuer}:{account}" +
                $"?secret={user.TotpSecretKey}" +
                $"&issuer={encodedIssuer}&digits=6&period=30";

            using var qrGenerator = new QRCodeGenerator();

            var qrData = qrGenerator.CreateQrCode(
                otpAuthUrl,
                QRCodeGenerator.ECCLevel.Q);

            var pngQrCode = new PngByteQRCode(qrData);
            var qrBytes = pngQrCode.GetGraphic(20);

            return Convert.ToBase64String(qrBytes);
        }

        public async Task<bool> VerifyTotpSetupAsync(
            int userId, string otp)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null ||
                user.IsTotpEnabled ||
                string.IsNullOrWhiteSpace(user.TotpSecretKey))
            {
                return false;
            }

            var secretBytes = Base32Encoding.ToBytes(
                user.TotpSecretKey);

            var totp = new Totp(secretBytes);

            var isValid = totp.VerifyTotp(
                otp,
                out long matchedTimeStep,
                new VerificationWindow(previous: 1, future: 1));

            if (!isValid)
                return false;

            user.IsTotpEnabled = true;

            await context.SaveChangesAsync();

            return true;
        }


        private string CreateLoginChallenge(User user, string purpose)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("token_type", purpose),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    configuration.GetValue<string>("AppSettings:Token")!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<string?> VerifyLoginOtpAsync(
            string challengeToken,
            string otp)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(challengeToken))
                return null;

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    configuration.GetValue<string>("AppSettings:Token")!));

            try
            {
                var principal = handler.ValidateToken(
                    challengeToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["AppSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["AppSettings:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    },
                    out _);

                if (principal.FindFirst("token_type")?.Value
                    != "login_challenge")
                {
                    return null;
                }

                var userIdValue = principal
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdValue, out var userId))
                    return null;

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user is null ||
                    !user.IsTotpEnabled ||
                    string.IsNullOrWhiteSpace(user.TotpSecretKey))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(otp) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(
                        otp, @"^\d{6}$"))
                {
                    return null;
                }

                var secret = Base32Encoding.ToBytes(user.TotpSecretKey);
                var totp = new Totp(secret);

                var isValid = totp.VerifyTotp(
                    otp,
                    out _,
                    new VerificationWindow(previous: 1, future: 1));

                if (!isValid)
                    return null;

                return CreateToken(user);
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }


        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Roles),
                new Claim("token_type", "access")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
