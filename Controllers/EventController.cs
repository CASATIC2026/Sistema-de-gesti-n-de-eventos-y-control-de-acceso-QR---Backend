using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            if (dto.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest(ApiResponse<object>.Fail("La fecha del evento debe ser futura.", 400));

            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                MaxCapacity = dto.MaxCapacity,
                Location = dto.Location,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = newEvent.Id },
                ApiResponse<Event>.Ok(newEvent, "Evento creado exitosamente")
            );
        }

        // GET: api/event
        /// <summary>
        /// Obtiene una lista de todos los eventos, ordenados por fecha. Incluye eventos activos e inactivos.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de eventos obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Where(e => e.IsActive)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(ApiResponse<List<Event>>.Ok(events, "Lista de eventos obtenida exitosamente"));
        }

        // GET: api/event/{id}
        /// <summary>
        /// Obtiene los detalles de un evento específico por su ID, incluyendo la lista de tickets asociados.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Evento encontrado y detalles retornados exitosamente.</response>
        /// <response code="404">Evento no encontrado.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
                return NotFound(ApiResponse<object>.Fail("Evento no encontrado.", 404));

            return Ok(ApiResponse<Event>.Ok(eventEntity, "Evento encontrado exitosamente"));
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
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<object>.Fail("El DTO es requerido.", 400));

            if (dto.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest(ApiResponse<object>.Fail("La fecha del evento debe ser futura.", 400));

            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound(ApiResponse<object>.Fail("Evento no encontrado.", 404));

            eventEntity.Name = dto.Name;
            eventEntity.Description = dto.Description;
            eventEntity.EventDate = dto.EventDate;
            eventEntity.MaxCapacity = dto.MaxCapacity;
            eventEntity.Location = dto.Location;
            eventEntity.ImageUrl = dto.ImageUrl;
            eventEntity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Evento actualizado exitosamente"));
        }

        // DELETE lógico (desactivar)
        /// <summary>
        /// Desactiva un evento estableciendo su propiedad IsActive a false. El evento no se elimina físicamente de la base de datos.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="204">Evento desactivado exitosamente.</response>
        /// <response code="404">Evento no encontrado.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateEvent(Guid id)
        {
            var eventEntity = await _context.Events.FindAsync(id);

            if (eventEntity == null)
                return NotFound(ApiResponse<object>.Fail("Evento no encontrado.", 404));

            eventEntity.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Evento desactivado exitosamente"));
        }

        /// <summary>
        /// Obtiene estadísticas de un evento específico, incluyendo la cantidad de tickets registrados, 
        /// la cantidad de tickets utilizados para ingreso, y la cantidad de tickets 
        /// restantes disponibles según la capacidad máxima del evento.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetEventStats(Guid id)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventEntity == null)
                return NotFound(ApiResponse<object>.Fail("Evento no encontrado.", 404));

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

            return Ok(ApiResponse<EventStatsDto>.Ok(stats, "Estadísticas del evento obtenidas exitosamente"));
        }
    }
}
