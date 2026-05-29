using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public required string Email { get; set; }
    }
}

