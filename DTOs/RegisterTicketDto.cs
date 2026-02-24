using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    public class RegisterTicketDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
    }
}