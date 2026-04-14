using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   
    /// <summary>
    /// DTO utilizado para la creación de un nuevo evento. Este modelo se utiliza para recibir los datos necesarios para crear un evento en el sistema.
    /// </summary>
    public class CreateEventDto
    {
        /// <summary>
        /// Nombre del evento, que es un campo obligatorio para identificar el evento.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento, que puede incluir detalles adicionales sobre el evento.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, que es un campo obligatorio para establecer cuándo se llevará a cabo el evento. 
        /// </summary>
        [Required]
        public required DateOnly EventDate { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes al evento, que es un campo obligatorio para establecer un límite en la 
        /// cantidad de personas que pueden asistir al evento. 
        /// </summary>
        [Required]
        [Range(1, 5000, ErrorMessage = "La capacidad debe estar entre 1 y 5000.")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación del evento, que es un campo opcional para proporcionar información sobre dónde se llevará a cabo el evento. 
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// URL de la imagen del evento.
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}