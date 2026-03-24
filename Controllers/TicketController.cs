using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;
using EventAccessControl.API.Services;
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
        private readonly TokenService _tokenService;
        private readonly QRService _qrService;
        private readonly EmailService _emailService;

        /// <summary>
        /// Constructor del controlador de tickets, inyectando el contexto de la base de datos, el servicio de generación de tokens, el servicio de generación de códigos QR y el servicio de envío de correos electrónicos.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenService"></param>
        /// <param name="qrService"></param>
        /// <param name="emailService"></param>
        public TicketController(ApplicationDbContext context, TokenService tokenService, QRService qrService, EmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _qrService = qrService;
            _emailService = emailService;
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

            // Crear ticket con nuevo ID
            var ticketId = Guid.NewGuid();

            // Generar token JWT
            var token = _tokenService.GenerateTicketToken(
                ticketId,
                dto.EventId,
                dto.UserEmail
            );

            // Hash del token usando SHA256
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                var tokenHash = Convert.ToBase64String(hashedBytes);

                var ticket = new Ticket
                {
                    Id = ticketId,
                    EventId = dto.EventId,
                    UserEmail = dto.UserEmail,
                    TokenHash = tokenHash,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var qrBase64 = _qrService.GenerateQRCodeBase64(token);

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                try
                {
                    await _emailService.SendQrEmail(ticket.UserEmail, qrBase64);
                }
                catch (Exception ex)
                {
                    // Log the error (puedes agregar un logger aquí)
                    Console.WriteLine($"Error al enviar email: {ex.Message}");
                    // El ticket se registró exitosamente, pero el email falló
                    return StatusCode(StatusCodes.Status206PartialContent, new
                    {
                        Message = "Registro exitoso, pero el email no pudo ser enviado",
                        TicketId = ticket.Id,
                        Error = ex.Message
                    });
                }

                return Ok(new
                {
                    Message = "Registro exitoso",
                    TicketId = ticket.Id
                });
            }
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

        /// <summary>
        /// Valida la entrada de un ticket utilizando el token JWT del código QR. Verifica que el token sea válido, que el ticket exista, y que no haya sido utilizado previamente.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("validate-entry")]
        public async Task<IActionResult> ValidateEntry([FromBody] ValidateEntryDto dto)
        {
            var principal = _tokenService.ValidateToken(dto.Token);

            if (principal == null)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    Result = "INVALID",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return BadRequest("QR inválido.");
            }
            

            var ticketId = Guid.Parse(principal.FindFirst("ticketId")!.Value);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    Result = "INVALID",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return NotFound("Ticket no encontrado.");
            }

            if (ticket.IsUsed)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    TicketId = ticket.Id,
                    Result = "DUPLICATE",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Ticket ya utilizado",
                    usedAt = ticket.UsedAt
                });
            }

            ticket.IsUsed = true;
            ticket.UsedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("El ticket fue validado simultáneamente.");
            }

            _context.CheckInLogs.Add(new CheckInLog
            {
                TicketId = ticket.Id,
                Result = "SUCCESS",
                DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Acceso permitido",
                ticketId = ticket.Id,
                time = ticket.UsedAt
            });
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