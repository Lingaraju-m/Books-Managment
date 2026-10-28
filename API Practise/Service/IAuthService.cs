using API_Practise.Entities;
using API_Practise.Models;

namespace API_Practise.Service
{
    public interface IAuthService
    {
        Task<User> RegisterAync(userDto request);

        Task<LoginChallengeDto> LoginAsync(userDto request);

        Task<string?> GenerateTotpQrCodeAsync(int userId);

        Task<bool> VerifyTotpSetupAsync(int userId, string otp);

        Task<string?> VerifyLoginOtpAsync(string challengeToken, string otp);
    }
}
