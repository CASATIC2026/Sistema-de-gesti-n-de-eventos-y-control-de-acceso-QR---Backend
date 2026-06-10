using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Data;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _context.Users
            .Where(u => u.Id == Guid.Parse(userId))
            .Select(u => new
            {
                u.Email,
                u.Role,
                u.BirthDate,
                u.Gender,
                u.ProfileCompleted
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Completa el perfil del usuario autenticado guardando fecha de nacimiento y género.
    /// Solo puede ejecutarse una vez (si el perfil ya está completo, retorna 400).
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

        if (user == null)
            return NotFound();

        // Validar que la fecha de nacimiento sea razonable
        // Validar que la fecha de nacimiento sea razonable
if (dto.BirthDate > DateOnly.FromDateTime(DateTime.Today) ||
    dto.BirthDate < new DateOnly(1900, 1, 1))
{
    return BadRequest(new { message = "Fecha de nacimiento inválida." });
}

if (string.IsNullOrWhiteSpace(dto.Gender))
{
    return BadRequest(new { message = "El género es requerido." });
}

user.BirthDate = dto.BirthDate;
user.Gender = dto.Gender;
user.ProfileCompleted = true;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Perfil completado correctamente." });
    }
}