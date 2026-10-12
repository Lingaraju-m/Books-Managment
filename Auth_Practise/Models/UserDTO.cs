using System.ComponentModel.DataAnnotations;

namespace Auth_Practise.Models
{
    public class UserDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
