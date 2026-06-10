using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   
    /// <summary>
    /// DTO para validar la entrada de un ticket utilizando el token JWT del código QR.  
    /// </summary>
    public class ValidateEntryDto
    {
        /// <summary>
        /// Token JWT del código QR que se va a validar. 
        /// </summary>
        [Required]
        public required string Token { get; set; }
    }
}