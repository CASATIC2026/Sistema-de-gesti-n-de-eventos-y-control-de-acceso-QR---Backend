using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;

namespace EventAccessControl.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/event
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
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/event/{id}
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
    }
}