using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   
    /// <summary>
    /// DTO utilizado para registrar un nuevo usuario en el sistema. Este modelo se utiliza para recibir los datos necesarios para crear un usuario en el sistema, 
    /// incluyendo el correo electrónico y la contraseña. 
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Correo electrónico del usuario. 
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario. Es un campo obligatorio y se utiliza para autenticar al usuario en el sistema.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public required string Password { get; set; }
    }
}