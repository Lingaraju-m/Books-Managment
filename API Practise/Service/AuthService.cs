using API_Practise.Data;
using API_Practise.Entities;
using API_Practise.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace API_Practise.Service
{
    public class AuthService(APIContext context, IConfiguration configuration) : IAuthService
    {
       
        public async Task<string> LoginAsync(userDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return CreateToken(user);
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

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Roles)
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
