using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string UserEmail { get; set; }
        public string TokenHash { get; set; }

        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Event Event { get; set; }
    }
}
