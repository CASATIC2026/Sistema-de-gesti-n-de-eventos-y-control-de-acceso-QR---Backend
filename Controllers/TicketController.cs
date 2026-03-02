using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace EventAccessControl.API.Controllers
{   
    /// <summary>
    /// Controlador para gestionar tickets, incluyendo registro de tickets para eventos específicos y consulta de tickets por evento. Implementa validaciones de aforo y registro 
    /// duplicado.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor que inyecta el contexto de la base de datos para acceder a los tickets y eventos.
        /// </summary>
        /// <param name="context"></param>
        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/ticket/register
        /// <summary>
        /// Registra un nuevo ticket para un evento específico utilizando el correo electrónico del usuario. Valida que el evento exista, esté activo, no haya ocurrido, y que no 
        /// se exceda el aforo. También evita registros duplicados por correo electrónico. Retorna un código único para el ticket registrado.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <response code="200">Registro exitoso y código del ticket retornado.</response>
        /// <response code="400">Datos inválidos, evento no activo, evento ya ocurrido, aforo completo o correo ya registrado.</response>
        /// <response code="404">Evento no encontrado.</response>
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
        /// <summary>
        /// Obtiene la lista de tickets registrados para un evento específico, incluyendo el correo del usuario, estado de uso y fecha de uso. Retorna una lista vacía si no hay 
        /// tickets registrados o si el evento no existe.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <response code="200">Lista de tickets obtenida exitosamente (puede estar vacía).</response>
        /// <response code="404">Evento no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
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