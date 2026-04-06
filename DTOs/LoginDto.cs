using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   /// <summary>
    /// DTO utilizado para iniciar sesión en el sistema. Este modelo se utiliza para recibir los datos necesarios para 
    /// iniciar sesión en el sistema, incluyendo el correo electrónico y la contraseña. 
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Correo electrónico del usuario. Es un campo obligatorio y se utiliza para identificar al usuario y para autenticar al usuario en el sistema.
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public required string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario. Es un campo obligatorio y se utiliza para autenticar al usuario en el sistema.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public required string Password { get; set; }
    }
}