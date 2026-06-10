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

        /// <summary>
        /// Distribución de géneros de los asistentes con perfil completo.
        /// </summary>
        public GenderDistributionDto GenderDistribution { get; set; } = new();

        /// <summary>
        /// Edad promedio de los asistentes con fecha de nacimiento registrada.
        /// Null si no hay datos disponibles.
        /// </summary>
        public double? AvgAge { get; set; }

        /// <summary>
        /// Distribución de rangos de edad.
        /// </summary>
        public Dictionary<string, int> AgeRanges { get; set; } = new();
    }

    public class GenderDistributionDto
    {
        public int Male { get; set; }
        public int Female { get; set; }
        public int Other { get; set; }
        public int PreferNotToSay { get; set; }
        public int SinDato { get; set; }
    }
}