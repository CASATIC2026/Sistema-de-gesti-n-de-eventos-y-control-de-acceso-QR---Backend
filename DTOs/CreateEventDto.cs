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
        public string Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento, que puede incluir detalles adicionales sobre el evento, como el tema, los oradores, o cualquier otra información relevante. 
        /// Este campo es opcional y tiene una longitud máxima de 1000 caracteres para permitir una descripción detallada sin exceder un límite razonable.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, que es un campo obligatorio para establecer cuándo se llevará a cabo el evento. Se utiliza el tipo DateOnly para representar solo la fecha sin la hora, 
        /// ya que no es necesario incluir información de hora para este tipo de eventos.
        /// </summary>
        [Required]
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes al evento, que es un campo obligatorio para establecer un límite en la cantidad de personas que pueden asistir al evento. 
        /// </summary>
        [Required]
        [Range(1, 100000)]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación del evento, que es un campo opcional para proporcionar información sobre dónde se llevará a cabo el evento. 
        /// Este campo puede incluir detalles como la dirección, el nombre del lugar, o cualquier otra información relevante sobre la ubicación del evento.
        /// </summary>
        public string? Location { get; set; }
    }
}