namespace EventAccessControl.API.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateOnly EventDate { get; set; }
        public int MaxCapacity { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Ticket>? Tickets { get; set; }
    }
}
