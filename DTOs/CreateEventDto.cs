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
         [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
        public required string Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento, que puede incluir detalles adicionales sobre el evento.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, que es un campo obligatorio para establecer cuándo se llevará a cabo el evento. 
        /// </summary>
         [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        //public required DateOnly EventDate { get; set; }
        public DateTimeOffset StartDateTime { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    public DateTimeOffset EndDateTime { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes al evento, que es un campo obligatorio para establecer un límite en la 
        /// cantidad de personas que pueden asistir al evento. 
        /// </summary>
        [Required(ErrorMessage = "La capacidad máxima es obligatoria.")]
    [Range(1, 5000, ErrorMessage = "La capacidad debe estar entre 1 y 5000 personas.")]
         public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación del evento, que es un campo opcional para proporcionar información sobre dónde se llevará a cabo el evento. 
        /// </summary>
         [MaxLength(300, ErrorMessage = "La ubicación no puede exceder los 300 caracteres.")]
        public string? Location { get; set; }

        /// <summary>
        /// URL de la imagen del evento.
        /// </summary>
         [Url(ErrorMessage = "La URL de la imagen no es válida.")]
    public string? ImageUrl { get; set; }
}
}