namespace EventAccessControl.API.Models
{   
    /// <summary>
    /// Modelo que representa un registro de intento de ingreso al evento utilizando un ticket. Se utiliza para llevar un historial de los intentos de ingreso, incluyendo la fecha y 
    /// hora del intento, el resultado del intento (éxito o fallo), y cualquier información adicional relevante, como el dispositivo utilizado para el intento.
    /// </summary>
    public class CheckInLog
    {
        /// <summary>
        /// Identificador único del registro de intento de ingreso, generado automáticamente. Se utiliza para referenciar el registro en operaciones de consulta y auditoría.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del ticket utilizado en el intento de ingreso. Es una clave foránea que referencia la entidad Ticket. 
        /// </summary>
        public Guid? TicketId { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el intento de ingreso al evento utilizando el ticket. 
        /// </summary>
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Resultado del intento de ingreso, indicando si fue exitoso o fallido.  
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Información adicional relevante sobre el intento de ingreso, como el dispositivo utilizado para el intento, la ubicación del intento, o cualquier otro detalle que pueda
        /// ser útil para auditorías o análisis posteriores. 
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Propiedad de navegación que establece la relación entre el registro de intento de ingreso y el ticket utilizado en ese intento. 
        /// </summary>
        public Ticket? Ticket { get; set; }
    }
}