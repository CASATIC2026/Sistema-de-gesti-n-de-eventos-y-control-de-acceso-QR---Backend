namespace EventAccessControl.API.Models
{
    /// <summary>
    /// Modelo que representa un evento en el sistema. Contiene información básica del evento como nombre, descripción, fecha, capacidad máxima, ubicación y estado de actividad. 
    /// Además, incluye una colección de tickets asociados al evento. Este modelo se utiliza para gestionar los eventos disponibles
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Identificador único del evento, generado automáticamente. Se utiliza para referenciar el evento en operaciones de consulta, registro de tickets y validación de acceso.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del evento. Es un campo obligatorio que se utiliza para identificar el evento y para mostrarlo a los usuarios. Se recomienda que el nombre sea descriptivo y 
        /// único para facilitar la identificación del evento.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento. Se utiliza para proporcionar información adicional sobre el evento, como detalles del programa, ponentes, actividades, etc. No es un 
        /// campo obligatorio, pero puede ser útil para los usuarios al momento de decidir registrarse en un evento.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, representada como DateOnly para almacenar solo la parte de la fecha sin la hora. Se utiliza para programar el evento y para validar que los tickets se 
        /// registren para eventos futuros. Es un campo obligatorio y debe ser una fecha futura al momento de la creación del evento. Se utiliza para mostrar la fecha del evento a 
        /// los usuarios y para realizar validaciones relacionadas con la fecha del evento, como evitar registros para eventos que ya ocurrieron.
        /// </summary>
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes para el evento. Se utiliza para controlar el aforo del evento y para validar que no se registren más tickets de los permitidos. Es un campo 
        /// obligatorio y debe ser un número positivo. Se utiliza para mostrar la capacidad del evento a los usuarios y para realizar validaciones relacionadas con el aforo, como 
        /// evitar registros cuando se alcanza la capacidad máxima del evento.
         /// 
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación opcional del evento. Se utiliza para proporcionar información sobre dónde se llevará a cabo el evento, como la dirección, el nombre del lugar, o cualquier 
        /// detalle relevante sobre la ubicación. 
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Indica si el evento está activo o no. Un evento activo es aquel que está disponible para que los usuarios se registren y asistan. Un evento inactivo es aquel que ha sido 
        /// desactivado, lo que significa que los usuarios no pueden registrarse ni asistir al evento. 
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha y hora en que el evento fue creado. Se establece automáticamente al momento de la creación del evento. Se utiliza para llevar un registro de cuándo se creó el 
        /// evento y para auditorías posteriores.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Colección de tickets asociados al evento. Se utiliza para establecer la relación entre el evento y los tickets registrados para ese evento. 
        /// </summary>
        public ICollection<Ticket>? Tickets { get; set; }
    }
}
