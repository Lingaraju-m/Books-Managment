using System.ComponentModel.DataAnnotations;

namespace Auth_Practise.Models
{
    public class AllUsersDto
    {
        
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}

