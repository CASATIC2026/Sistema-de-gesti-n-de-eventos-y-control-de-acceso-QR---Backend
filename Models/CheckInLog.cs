namespace EventAccessControl.API.Models
{
    public class CheckInLog
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        public string Result { get; set; }
        public string? DeviceInfo { get; set; }

        public Ticket Ticket { get; set; }
    }
}