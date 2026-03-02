using System.ComponentModel.DataAnnotations;

namespace EventAccessControl.API.DTOs
{   
    /// <summary>
    /// DTO utilizado para actualizar la información de un evento existente. Este modelo se utiliza para recibir los datos necesarios para modificar un evento en el sistema,
    /// incluyendo el nombre, la descripción, la fecha, la capacidad máxima, la ubicación y el estado activo del evento.
    /// </summary>
    public class UpdateEventDto
    {   
        /// <summary>
        /// Nombre del evento, que es un campo obligatorio para identificar el evento. Este campo se utiliza para actualizar el nombre del evento en el sistema, lo que permite 
        /// mantener la información del evento actualizada y relevante para los usuarios.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Descripción opcional del evento, que puede incluir detalles adicionales sobre el evento, como el tema, los oradores, o cualquier otra información relevante. 
        /// Se utiliza para actualizar la descripción del evento en el sistema, lo que permite mantener la información del evento actualizada y relevante para los usuarios.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha del evento, que es un campo obligatorio para establecer cuándo se llevará a cabo el evento. Se utiliza el tipo DateOnly para representar solo la fecha sin la hora, 
        /// ya que no es necesario incluir información de hora para este tipo de eventos. 
        /// </summary>
        [Required]
        public DateOnly EventDate { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes al evento, que es un campo obligatorio para establecer un límite en la cantidad de personas que pueden asistir al evento. Se utiliza el 
        /// tipo int y se valida para asegurarse de que el valor esté dentro de un rango razonable (1 a 100000) para evitar errores en la actualización del evento.
        /// </summary>
        [Required]
        [Range(1, 100000)]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Ubicación del evento, que es un campo opcional para proporcionar información sobre dónde se llevará a cabo el evento. Este campo puede incluir detalles como la 
        /// dirección, el nombre del lugar, o cualquier otra información relevante sobre la ubicación del evento.  
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Indica si el evento está activo o no, lo que es un campo obligatorio para establecer el estado del evento en el sistema. Este campo se utiliza para actualizar el estado 
        /// del evento, lo que permite controlar la visibilidad y la disponibilidad del evento para los usuarios. Un evento inactivo puede ser ocultado de las listas de eventos 
        /// disponibles o puede ser marcado como cerrado para nuevas inscripciones, dependiendo de la lógica de negocio implementada en el sistema.
        /// </summary>
        public bool IsActive { get; set; }
    }
}