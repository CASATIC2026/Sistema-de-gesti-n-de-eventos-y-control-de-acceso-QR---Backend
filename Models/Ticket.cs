using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.Models
{
    /// <summary>
    /// Modelo que representa un ticket registrado para un evento específico, asociado a un correo electrónico de usuario. Incluye información sobre el estado de uso del ticket, 
    /// fecha de uso, y una referencia al evento al que pertenece. Implementa control de concurrencia optimista mediante RowVersion para evitar registros duplicados y garantizar 
    /// la integridad de los datos en escenarios de alta concurrencia.
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
        public string UserEmail { get; set; }

        /// <summary>
        /// Hash del código único generado para el ticket. Se utiliza para validar la autenticidad del ticket durante los intentos de ingreso al evento. Es un valor que se genera al 
        /// momento de registrar el ticket y se almacena en la base de datos para su posterior comparación durante
        /// </summary>
        public string TokenHash { get; set; }

        /// <summary>
        /// Indica si el ticket ha sido utilizado para ingresar al evento. Es un valor booleano que se establece en true cuando el ticket es validado exitosamente en un intento de 
        /// ingreso. Se utiliza para evitar el uso múltiple de un mismo ticket.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Fecha y hora en que el ticket fue utilizado para ingresar al evento. Es un valor nullable que se establece cuando IsUsed es true. Se utiliza para llevar un registro de 
        /// cuándo se utilizó el ticket y para auditorías posteriores.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Fecha y hora en que el ticket fue creado. Se establece automáticamente al momento de la creación del ticket. Se utiliza para llevar un registro de cuándo se registró el 
        /// ticket y para auditorías posteriores.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Campo de control de concurrencia optimista. Se utiliza para evitar registros duplicados y garantizar la integridad de los datos en escenarios de alta concurrencia.
        /// </summary>
        public Event Event { get; set; }
    }
}
