using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.Models
{
    /// <summary>
    /// Modelo que representa un ticket registrado para un evento específico, asociado a un correo electrónico de usuario. 
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Identificador único del ticket, generado automáticamente. Se utiliza para referenciar el ticket en operaciones de consulta y validación.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del evento al que pertenece el ticket. Es una clave foránea que referencia la entidad Event. Es obligatorio para asociar el ticket a un evento específico.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Correo electrónico del usuario para el cual se registra el ticket. Es obligatorio y se utiliza para validar registros duplicados y para identificar al usuario asociado 
        /// al ticket.
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Hash del código único generado para el ticket. 
        /// </summary>
        public string? TokenHash { get; set; }

        /// <summary>
        /// Indica si el ticket ha sido utilizado para ingresar al evento. Es un valor booleano que se establece en 
        /// true cuando el ticket es validado exitosamente en un intento de ingreso. 
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Fecha y hora en que el ticket fue utilizado para ingresar al evento. 
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Fecha y hora en que el ticket fue creado. 
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado del envío del correo electrónico con el código del ticket. Puede tener valores como "PENDIENTE", 
        /// "ENVIADO", "FALLIDO", etc. 
        /// </summary>
        public string EmailStatus { get; set; } = "PENDIENTE";

        /// <summary>
        /// Contador de reintentos de envío del correo electrónico. Se incrementa cada vez que se intenta enviar el 
        /// correo electrónico con el código del ticket. 
        public int EmailRetryCount { get; set; } = 0;
        
        /// <summary>
        /// Fecha y hora del último intento de envío del correo electrónico. Se actualiza cada vez que se intenta 
        /// enviar el correo electrónico con el código del ticket. 
        /// </summary>
        public DateTime? EmailSentAt { get; set; }

        /// <summary>
        /// Campo de control de concurrencia optimista. Se utiliza para evitar registros duplicados y garantizar la 
        /// integridad de los datos en escenarios de alta concurrencia.
        /// </summary>
        public Event? Event { get; set; }
    }
}
