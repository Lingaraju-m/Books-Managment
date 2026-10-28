using System.ComponentModel.DataAnnotations;

namespace API_Practise.Models
{
    public class VerifyLoginOtpDto
    {
        [Required]
        public string ChallengeToken { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "OTP must contain exactly 6 digits.")]
        public string Otp { get; set; } = string.Empty;
    }
}
