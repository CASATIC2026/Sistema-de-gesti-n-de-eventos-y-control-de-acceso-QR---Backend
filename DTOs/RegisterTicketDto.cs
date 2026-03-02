using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    /// <summary>
    /// DTO utilizado para registrar un nuevo ticket para un evento específico. Este modelo se utiliza para recibir los datos necesarios para crear un ticket en el sistema, 
    /// incluyendo el identificador del evento al que se asociará el ticket y el correo electrónico del usuario que posee el ticket. El campo EventId es obligatorio para establecer 
    /// la relación entre el ticket y el evento correspondiente, mientras que el campo UserEmail es obligatorio para identificar al usuario que posee el ticket y para enviar 
    /// notificaciones relacionadas con el evento. Este DTO es esencial para la funcionalidad de registro de tickets en el sistema de control de acceso a eventos.
    /// </summary>
    public class RegisterTicketDto
    {   
        /// <summary>
        /// Identificador del evento al que se asociará el ticket. Es un campo obligatorio que se utiliza para establecer la relación entre el ticket y el evento correspondiente, 
        /// lo que permite realizar consultas relacionadas con los tickets de un evento específico y para gestionar la asignación de tickets a eventos en el sistema.
        /// </summary>
        [Required]
        public Guid EventId { get; set; }


        /// <summary>
        /// Correo electrónico del usuario que posee el ticket. Es un campo obligatorio que se utiliza para identificar al usuario que posee el ticket y para enviar notificaciones 
        /// relacionadas con el evento, como recordatorios, actualizaciones o cualquier otra comunicación relevante. 
        /// </summary>
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
    }
}