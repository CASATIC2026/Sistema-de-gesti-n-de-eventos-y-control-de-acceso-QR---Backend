using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.Models
{
    /// <summary>
    /// Modelo que representa un usuario en el sistema. Contiene información básica del usuario como correo electrónico, contraseña hash, rol y fecha de creación.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único del usuario, generado automáticamente. Se utiliza para referenciar el usuario en operaciones de consulta y registro.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Correo electrónico del usuario. Es un campo obligatorio y se utiliza para identificar al usuario y para enviar notificaciones relacionadas con el usuario.
        /// </summary>
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// Contraseña hash del usuario. Es un campo obligatorio y se utiliza para autenticar al usuario en el sistema.
        /// </summary>
        [Required]
        public string? PasswordHash { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }

        public bool ProfileCompleted { get; set; } = false;

        /// <summary>
        /// Rol del usuario. Es un campo obligatorio y se utiliza para identificar el rol del usuario en el sistema.
        /// </summary>
        public string Role { get; set; } = "User"; // Admin / User

        /// <summary>
        /// Fecha de creación del usuario. Se establece automáticamente al momento de la creación del usuario. 
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //reset password
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}