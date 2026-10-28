namespace API_Practise.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        public string? TotpSecretKey { get; set; }

        public bool IsTotpEnabled { get; set; } = false;
    }
}
