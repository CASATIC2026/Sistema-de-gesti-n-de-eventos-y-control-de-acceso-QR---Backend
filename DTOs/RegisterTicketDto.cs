using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    /// <summary>
    /// DTO utilizado para registrar un nuevo ticket para un evento específico. Este modelo se utiliza para recibir 
    /// los datos necesarios para crear un ticket en el sistema, incluyendo el identificador del evento al que se 
    /// asociará el ticket y el correo electrónico del usuario que posee el ticket. 
    /// </summary>
    public class RegisterTicketDto
    {   
        /// <summary>
        /// Identificador del evento al que se asociará el ticket. 
        /// </summary>
        [Required]
        public Guid EventId { get; set; }
        /// <summary>
        /// Correo electrónico opcional proporcionado en el cuerpo de la petición.
        /// Si no se proporciona, se intentará obtener del token de autenticación.
        /// </summary>
        [EmailAddress]
        public string? UserEmail { get; set; }
    }
}