using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;

namespace EventAccessControl.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar eventos, incluyendo creación, consulta, actualización y desactivación lógica.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de la base de datos para acceder a los eventos.
        /// </summary>
        /// <param name="context"></param>
        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/event
        /// <summary>
        /// Crea un nuevo evento con los datos proporcionados en el DTO. La fecha del evento debe ser futura.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <response code="201">Evento creado exitosamente.</response>
        /// <response code="400">Datos inválidos o fecha del evento no es futura.</response>
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            if (dto.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest("La fecha del evento debe ser futura.");

            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                MaxCapacity = dto.MaxCapacity,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // GET: api/event
        /// <summary>
        /// Obtiene una lista de todos los eventos, ordenados por fecha. Incluye eventos activos e inactivos.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de eventos obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/event/{id}
        /// <summary>
        /// Obtiene los detalles de un evento específico por su ID, incluyendo la lista de tickets asociados.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Evento encontrado y detalles retornados exitosamente.</response>
        /// <response code="404">Evento no encontrado.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
                return NotFound("Evento no encontrado.");

            return Ok(eventEntity);
        }

        // PUT: api/event/{id}
        /// <summary>
        /// Actualiza los detalles de un evento existente. Permite modificar el nombre, descripción, fecha, capacidad máxima, ubicación y estado activo.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <response code="204">Evento actualizado exitosamente.</response>
        /// <response code="400">Datos inválidos.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
        {
            if (dto == null)
                return BadRequest("El DTO es requerido.");

            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound("Evento no encontrado.");

            eventEntity.Name = dto.Name;
            eventEntity.Description = dto.Description;
            eventEntity.EventDate = dto.EventDate;
            eventEntity.MaxCapacity = dto.MaxCapacity;
            eventEntity.Location = dto.Location;
            eventEntity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE lógico (desactivar)
        /// <summary>
        /// Desactiva un evento estableciendo su propiedad IsActive a false. El evento no se elimina físicamente de la base de datos.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="204">Evento desactivado exitosamente.</response>
        /// <response code="404">Evento no encontrado.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateEvent(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound("Evento no encontrado.");

            eventEntity.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtiene estadísticas de un evento específico, incluyendo la cantidad de tickets registrados, la cantidad de tickets utilizados para ingreso, y la cantidad de tickets 
        /// restantes disponibles según la capacidad máxima del evento. Esta información es útil para monitorear el uso de los tickets y para gestionar el aforo del evento en 
        /// tiempo real. El endpoint retorna un DTO con las estadísticas del evento solicitado.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetEventStats(Guid id)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
                return NotFound("Evento no encontrado.");

            var ticketsRegistered = await _context.Tickets
                .CountAsync(t => t.EventId == id);

            var checkedIn = await _context.Tickets
                .CountAsync(t => t.EventId == id && t.IsUsed);

            var stats = new EventStatsDto
            {
                EventId = eventEntity.Id,
                Capacity = eventEntity.MaxCapacity,
                TicketsRegistered = ticketsRegistered,
                CheckedIn = checkedIn,
                Remaining = eventEntity.MaxCapacity - checkedIn
            };

            return Ok(stats);
        }
    }
}
