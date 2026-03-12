namespace EventAccessControl.API.DTOs
{
    public class EventStatsDto
    {
        public Guid EventId { get; set; }

        public int Capacity { get; set; }

        public int TicketsRegistered { get; set; }

        public int CheckedIn { get; set; }

        public int Remaining { get; set; }
    }
}