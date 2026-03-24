namespace EventAccessControl.API.DTOs
{
    /// <summary>
    /// DTO para representar las estadísticas de un evento, incluyendo la capacidad total, el número de tickets registrados, el número de entradas validadas, y el número de entradas 
    /// restantes. Este DTO se utiliza para mostrar información relevante sobre el estado del evento en términos de acceso y control de entradas.
    /// </summary>
    public class EventStatsDto
    {
        /// <summary>
        /// Identificador único del evento para el cual se están mostrando las estadísticas.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Capacidad total del evento, es decir, el número máximo de asistentes permitidos. Esta información es crucial para controlar el acceso y evitar sobrecupo en el evento.
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        /// Número de tickets que han sido registrados para el evento. Este número puede ser igual o menor a la capacidad total, y es un indicador de cuántas personas han mostrado 
        /// interés en asistir al evento.
        /// </summary>
        public int TicketsRegistered { get; set; }

        /// <summary>
        /// Número de entradas que han sido validadas en la entrada del evento. Este número representa cuántas personas han ingresado efectivamente al evento, y es un indicador 
        /// clave para el control de acceso.
        /// </summary>
        public int CheckedIn { get; set; }

        /// <summary>
        /// Número de entradas restantes, calculado como la diferencia entre la capacidad total y el número de entradas validadas. Este número es importante para el personal del 
        /// evento, ya que les permite saber cuántas personas más pueden ingresar antes de alcanzar la capacidad máxima.
        /// </summary>
        public int Remaining { get; set; }
    }
}