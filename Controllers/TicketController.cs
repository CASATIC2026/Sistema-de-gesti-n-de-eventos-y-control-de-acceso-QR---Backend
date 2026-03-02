using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace EventAccessControl.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/ticket/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterTicket(RegisterTicketDto dto)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == dto.EventId);

            if (eventEntity == null)
                return NotFound("Evento no encontrado.");

            if (!eventEntity.IsActive)
                return BadRequest("El evento no está activo.");

            if (eventEntity.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest("El evento ya ocurrió.");

            // Validar aforo
            var currentTickets = eventEntity.Tickets?.Count ?? 0;

            if (currentTickets >= eventEntity.MaxCapacity)
                return BadRequest("Aforo completo.");

            // Evitar registro duplicado por email
            var alreadyRegistered = await _context.Tickets
                .AnyAsync(t => t.EventId == dto.EventId &&
                               t.UserEmail == dto.UserEmail);

            if (alreadyRegistered)
                return BadRequest("Este correo ya está registrado en el evento.");

            // Generar código único (Mes 1)
            var rawCode = Guid.NewGuid().ToString();

            // Crear hash del código (simulando seguridad futura)
            var tokenHash = GenerateSha256Hash(rawCode);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = dto.EventId,
                UserEmail = dto.UserEmail,
                TokenHash = tokenHash,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Registro exitoso",
                TicketId = ticket.Id,
                Code = rawCode // Solo para pruebas (luego será JWT)
            });
        }

        // GET: api/ticket/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetTicketsByEvent(Guid eventId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.EventId == eventId)
                .Select(t => new
                {
                    t.Id,
                    t.UserEmail,
                    t.IsUsed,
                    t.UsedAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        private string GenerateSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}