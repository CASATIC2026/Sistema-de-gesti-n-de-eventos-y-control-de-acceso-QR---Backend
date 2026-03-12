using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    public class ValidateEntryDto
    {
        [Required]
        public string Token { get; set; }
    }
}