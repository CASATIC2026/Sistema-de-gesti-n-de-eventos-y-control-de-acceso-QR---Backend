using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using EventAccessControl.API.DTOs;
using EventAccessControl.API.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;

namespace EventAccessControl.API.Controllers
{   
    /// <summary>
    /// Controlador para gestionar tickets, incluyendo registro de tickets para eventos específicos y consulta de tickets por evento. Implementa validaciones de aforo y registro 
    /// duplicado.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        /// Registra un nuevo ticket para un evento específico utilizando el correo electrónico del usuario. 
        /// Valida que el evento exista, esté activo, no haya ocurrido, y que no 
        /// se exceda el aforo. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <response code="200">Registro exitoso y código del ticket retornado.</response>
        /// <response code="400">Datos inválidos, evento no activo, evento ya ocurrido, aforo completo o correo ya registrado.</response>
        /// <response code="404">Evento no encontrado.</response>
        [Authorize(Roles = "User")]
        [EnableRateLimiting("RegisterTicketPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterTicket(RegisterTicketDto dto)
        {
            // Obtener email desde DTO o claims
            var claimEmail = User.FindFirst("email")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type != null && c.Type.ToLower().Contains("email"))?.Value;

            var userEmail = string.IsNullOrWhiteSpace(dto.UserEmail)
                ? claimEmail
                : dto.UserEmail!.Trim();

            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized(ApiResponse<object>.Fail("No se pudo obtener el email del usuario.", 401));

            // Buscar evento
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == dto.EventId);

            if (eventEntity == null)
                return NotFound(ApiResponse<object>.Fail("Evento no encontrado.", 404));

            if (!eventEntity.IsActive)
                return BadRequest(ApiResponse<object>.Fail("El evento no está activo.", 400));

            if (eventEntity.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                return BadRequest(ApiResponse<object>.Fail("El evento ya ocurrió.", 400));

            // Validar aforo
            var currentTickets = await _context.Tickets.CountAsync(t => t.EventId == dto.EventId);

            if (currentTickets >= eventEntity.MaxCapacity)
                return BadRequest(ApiResponse<object>.Fail("Aforo completo.", 400));

            // buscar ticket existente
            var existing = await _context.Tickets
                .FirstOrDefaultAsync(t => t.EventId == dto.EventId && t.UserEmail == userEmail);

            if (existing != null)
            {
                if (existing.EmailStatus == "ENVIADO")
                {
                    return Ok(ApiResponse<object>.Ok(
                        new { ticketId = existing.Id },
                        "Ya tienes tu ticket, revisa tu correo"
                    ));
                }

                if (existing.EmailStatus == "FALLIDO")
                {
                    return await SendEmailFlow(existing, eventEntity);
                }
            }

            // Crear nuevo ticket
            var ticketId = Guid.NewGuid();

            var token = _tokenService.GenerateTicketToken(
                ticketId,
                dto.EventId,
                userEmail
            );

            var tokenHash = GenerateSha256Hash(token);

            var ticket = new Ticket
            {
                Id = ticketId,
                EventId = dto.EventId,
                UserEmail = userEmail,
                TokenHash = tokenHash,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                EmailStatus = "PENDIENTE"
            };

            _context.Tickets.Add(ticket);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Tickets_EventId_UserEmail") == true)
                {
                    // LIMPIAR entidades en memoria
                    _context.ChangeTracker.Clear();

                    // Ya existe → buscarlo y reutilizarlo
                    var existingTicket = await _context.Tickets
                        .FirstOrDefaultAsync(t => t.EventId == dto.EventId && t.UserEmail == userEmail);

                    if (existingTicket != null)
                    {
                        return await SendEmailFlow(existingTicket, eventEntity);
                    }

                    return BadRequest(ApiResponse<object>.Fail("El ticket ya existe.", 400));
                }

                throw;
            }

            return await SendEmailFlow(ticket, eventEntity, token);
        }

        private async Task<IActionResult> SendEmailFlow(Ticket ticket, Event eventEntity, string? existingToken = null)
        {
            try
            {
                var token = existingToken ?? _tokenService.GenerateTicketToken(
                    ticket.Id,
                    ticket.EventId,
                    ticket.UserEmail!
                );

                var qrBase64 = _qrService.GenerateQRCodeBase64(token);

                await _emailService.SendQrEmail(
                    ticket.UserEmail!,
                    qrBase64,
                    eventEntity.Name,
                    eventEntity.EventDate,
                    ticket.Id
                );

                ticket.EmailStatus = "ENVIADO";
                ticket.EmailSentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                ticket.EmailStatus = "FALLIDO";
                ticket.EmailRetryCount++;

                Console.WriteLine($"Error email: {ex.Message}");
            }

            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    ticketId = ticket.Id,
                    emailStatus = ticket.EmailStatus
                },
                ticket.EmailStatus == "ENVIADO"
                    ? "Registro exitoso, revisa tu correo"
                    : "Registro exitoso, pero el correo falló. Puedes intentar nuevamente"
            ));
        }

        /// <summary>
        /// Obtiene la lista de tickets registrados por el usuario autenticado.
        /// Lee el correo desde los claims del token JWT y retorna la información 
        /// de los tickets junto con los datos básicos del evento asociado.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Lista de tickets del usuario obtenida exitosamente.</response>
        /// <response code="401">No se pudo identificar al usuario desde el token.</response>
        [Authorize(Roles = "User")]
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            // 1. Obtener email desde los claims del token JWT de forma segura
            var userEmail = User.FindFirst("email")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type != null && c.Type.ToLower().Contains("email"))?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(ApiResponse<object>.Fail("No se pudo obtener la identidad del usuario.", 401));
            }

            // 2. Buscar los tickets en la base de datos que pertenezcan a este email
            // Usamos Include para traer la información del evento relacionado
            var myTickets = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.UserEmail == userEmail)
                .OrderByDescending(t => t.CreatedAt) // Mostramos los más recientes primero
                .Select(t => new
                {
                    TicketId = t.Id,
                    IsUsed = t.IsUsed,
                    UsedAt = t.UsedAt,
                    CreatedAt = t.CreatedAt,
                    EmailStatus = t.EmailStatus,
                    // Proyectamos solo la información pública y necesaria del evento
                    Event = new
                    {
                        EventId = t.Event!.Id,
                        Name = t.Event.Name,
                        EventDate = t.Event.EventDate,
                        IsActive = t.Event.IsActive
                    }
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(
                myTickets,
                "Tus tickets han sido obtenidos correctamente"
            ));
        }

        /// <summary>
        /// Obtiene la lista de tickets registrados para un evento específico con paginación.
        /// </summary>
        /// <param name="eventId">El identificador único del evento.</param>
        /// <param name="page">El número de página a consultar (por defecto 1).</param>
        /// <param name="pageSize">La cantidad de registros por página (por defecto 10, máximo 100).</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetTicketsByEvent(Guid eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // 1. Validaciones de seguridad para la paginación
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Límite máximo para evitar que pidan 1 millón de golpe

            // 2. Preparar la consulta base (sin ejecutarla todavía)
            var query = _context.Tickets.Where(t => t.EventId == eventId);

            // 3. Obtener el total de registros para la metadata
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // 4. Aplicar ordenamiento y paginación
            var tickets = await query
                .OrderByDescending(t => t.CreatedAt) // IMPORTANTE: Siempre ordena antes de paginar para tener resultados consistentes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.UserEmail,
                    t.IsUsed,
                    t.UsedAt,
                    t.CreatedAt
                })
                .ToListAsync();

            // 5. Estructurar la respuesta con los datos y la metadata de paginación
            var responseData = new
            {
                Items = tickets,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                }
            };

            return Ok(ApiResponse<object>.Ok(
                responseData,
                "Tickets obtenidos correctamente"
            ));
        }

        /// <summary>
        /// Valida la entrada de un ticket utilizando el token JWT del código QR. Verifica que el token sea válido, que el ticket exista, y que no haya sido utilizado previamente.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("validate-entry")]
        public async Task<IActionResult> ValidateEntry([FromBody] ValidateEntryDto dto)
        {
            var principal = _tokenService.ValidateToken(dto.Token);

            if (principal == null)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    Result = "INVALIDO",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return BadRequest(ApiResponse<object>.Fail("QR inválido.", 400));
            }

            var hash = GenerateSha256Hash(dto.Token);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (ticket == null)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    Result = "INVALIDO",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return NotFound(ApiResponse<object>.Fail("Ticket no encontrado.", 404));
            }

            if (ticket.IsUsed)
            {
                _context.CheckInLogs.Add(new CheckInLog
                {
                    TicketId = ticket.Id,
                    Result = "DUPLICADO",
                    DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                await _context.SaveChangesAsync();

                return BadRequest(ApiResponse<object>.Fail(
                    "Ticket ya utilizado",
                    400,
                    new { usedAt = ticket.UsedAt }
                ));
            }

            ticket.IsUsed = true;
            ticket.UsedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResponse<object>.Fail("El ticket fue validado simultáneamente.", 400));
            }

            _context.CheckInLogs.Add(new CheckInLog
            {
                TicketId = ticket.Id,
                Result = "VALIDO",
                DeviceInfo = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    ticketId = ticket.Id,
                    time = ticket.UsedAt
                },
                "Acceso permitido"
            ));
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