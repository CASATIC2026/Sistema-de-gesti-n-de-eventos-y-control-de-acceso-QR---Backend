using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User"; // Admin / User

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}