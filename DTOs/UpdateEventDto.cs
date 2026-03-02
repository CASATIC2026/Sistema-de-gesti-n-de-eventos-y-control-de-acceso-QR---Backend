using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateOnly EventDate { get; set; }

        [Required]
        [Range(1, 100000)]
        public int MaxCapacity { get; set; }

        public string? Location { get; set; }
        public bool IsActive { get; set; }
    }
}