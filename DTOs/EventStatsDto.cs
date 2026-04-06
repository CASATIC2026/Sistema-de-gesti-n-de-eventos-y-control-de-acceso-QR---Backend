namespace EventAccessControl.API.DTOs
{
    /// <summary>
    /// DTO para representar las estadísticas de un evento, incluyendo la capacidad total, el número de tickets 
    /// registrados, el número de entradas validadas, y el número de entradas restantes. 
    /// </summary>
    public class EventStatsDto
    {
        /// <summary>
        /// Identificador único del evento para el cual se están mostrando las estadísticas.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Capacidad total del evento, es decir, el número máximo de asistentes permitidos. 
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        /// Número de tickets que han sido registrados para el evento. 
        /// </summary>
        public int TicketsRegistered { get; set; }

        /// <summary>
        /// Número de entradas que han sido validadas en la entrada del evento. 
        /// </summary>
        public int CheckedIn { get; set; }

        /// <summary>
        /// Número de entradas restantes, calculado como la diferencia entre la capacidad total y el número de 
        /// entradas validadas.
        /// </summary>
        public int Remaining { get; set; }
    }
}