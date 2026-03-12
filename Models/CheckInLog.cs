namespace EventAccessControl.API.Models
{   
    /// <summary>
    /// Modelo que representa un registro de intento de ingreso al evento utilizando un ticket. Se utiliza para llevar un historial de los intentos de ingreso, incluyendo la fecha y 
    /// hora del intento, el resultado del intento (éxito o fallo), y cualquier información adicional relevante, como el dispositivo utilizado para el intento. Este modelo se 
    /// relaciona con el modelo Ticket a través de la propiedad TicketId, lo que permite asociar cada intento de ingreso con el ticket correspondiente. El registro de estos intentos 
    /// es útil para auditorías, análisis de seguridad y para proporcionar información detallada sobre el uso de los tickets en los eventos.
    /// </summary>
    public class CheckInLog
    {
        /// <summary>
        /// Identificador único del registro de intento de ingreso, generado automáticamente. Se utiliza para referenciar el registro en operaciones de consulta y auditoría.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del ticket utilizado en el intento de ingreso. Es una clave foránea que referencia la entidad Ticket. Es obligatorio para asociar el intento de ingreso con 
        /// un ticket específico y para realizar consultas relacionadas con los intentos de ingreso de un ticket en particular.
        /// </summary>
        public Guid? TicketId { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el intento de ingreso al evento utilizando el ticket. Se establece automáticamente al momento de crear el registro del intento de ingreso. 
        /// Se utiliza para llevar un registro cronológico de los intentos de ingreso y para auditorías.
        /// </summary>
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Resultado del intento de ingreso, indicando si fue exitoso o fallido. Es un campo obligatorio que se utiliza para analizar el uso de los tickets y para identificar 
        /// patrones de uso, intentos de ingreso fallidos, o posibles problemas con los tickets. 
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Información adicional relevante sobre el intento de ingreso, como el dispositivo utilizado para el intento, la ubicación del intento, o cualquier otro detalle que pueda
        /// ser útil para auditorías o análisis posteriores. 
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Propiedad de navegación que establece la relación entre el registro de intento de ingreso y el ticket utilizado en ese intento. Permite acceder a la información del ticket
        /// asociado al intento de ingreso, lo que es útil para realizar consultas relacionadas con los intentos de ingreso de un ticket en particular, o para analizar el uso de los 
        /// tickets en general.
        /// </summary>
        public Ticket? Ticket { get; set; }
    }
}