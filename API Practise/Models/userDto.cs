using System.ComponentModel.DataAnnotations;

namespace API_Practise.Models
{
    public class userDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
