namespace EventAccessControl.API.Models
{
    /// <summary>
    /// Modelo que representa un evento en el sistema. Contiene información básica del evento como nombre, descripción, fecha, capacidad máxima, ubicación y estado de actividad. 
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Identificador único del evento, generado automáticamente. Se utiliza para referenciar el evento en operaciones de consulta, registro de tickets y validación de acceso.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del evento. 
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento. Se utiliza para proporcionar información adicional sobre el evento, como detalles del programa, ponentes, actividades, etc. 
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, representada como DateOnly para almacenar solo la parte de la fecha sin la hora. Se utiliza para programar el evento y para validar que los tickets se 
        /// registren para eventos futuros. 
        /// </summary>
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes para el evento. 
         /// 
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación opcional del evento.  
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Indica si el evento está activo o no.  
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// URL de la imagen del evento.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Fecha y hora en que el evento fue creado. 
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Colección de tickets asociados al evento. Se utiliza para establecer la relación entre el evento y los 
        /// tickets registrados para ese evento. 
        /// </summary>
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
