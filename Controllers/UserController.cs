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
}