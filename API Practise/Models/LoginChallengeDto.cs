namespace API_Practise.Models
{
    public class LoginChallengeDto
    {
        public bool RequiresOtp { get; set; }

        public string? ChallengeToken { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
