using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   
    /// <summary>
    /// DTO para validar la entrada de un ticket utilizando el token JWT del código QR. Este DTO contiene una propiedad "Token" que es requerida y se utiliza para recibir el token 
    /// JWT enviado desde la aplicación móvil o el dispositivo de escaneo en la entrada del evento. El controlador de tickets utilizará este DTO para validar el token, verificar que el ticket exista, y asegurarse de que no haya sido utilizado previamente para permitir o denegar el acceso al evento. 
    /// </summary>
    public class ValidateEntryDto
    {
        /// <summary>
        /// Token JWT del código QR que se va a validar. Esta propiedad es requerida y debe contener el token generado para el ticket correspondiente. El controlador de tickets 
        /// utilizará este token para realizar la validación de la entrada al evento.
        /// </summary>
        [Required]
        public string Token { get; set; }
    }
}