using Microsoft.AspNetCore.Mvc;
using EventAccessControl.API.Data;
using EventAccessControl.API.Models;
using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Services;
using EventAccessControl.API.DTOs;

namespace EventAccessControl.API.Controllers
{   /// <summary>
    /// Controlador para gestionar autenticación y autorización en el sistema. Incluye endpoints para registrar nuevos 
    /// usuarios y para iniciar sesión en el sistema.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {   
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        private readonly EmailService _emailService;
        
        /// <summary>
        /// Constructor del controlador de autenticación, inyectando el contexto de la base de datos y el servicio de 
        /// generación de tokens.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenService"></param>
       
      public AuthController(
    ApplicationDbContext context,
    TokenService tokenService,
    EmailService emailService)
{
    _context = context;
    _tokenService = tokenService;
    _emailService = emailService;
}

        /// <summary>
        /// Registra un nuevo usuario en el sistema utilizando el correo electrónico y la contraseña proporcionados en 
        /// el DTO. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Validación automática
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Los datos enviados no son válidos.", 400, ModelState));
                
            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);

            if (exists)
                return BadRequest(ApiResponse<object>.Fail("El usuario ya existe.", 400));

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Usuario registrado"));
        }

        /// <summary>
        /// Inicia sesión en el sistema utilizando el correo electrónico y la contraseña proporcionados en el DTO. 
        /// Valida las credenciales del usuario y genera un token JWT para 
        /// el usuario.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Validación automática
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Los datos enviados no son válidos.", 400, ModelState));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(ApiResponse<object>.Fail("Usuario no encontrado.", 401));

            var isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isValid)
                return Unauthorized(ApiResponse<object>.Fail("Contraseña inválida.", 401));

            var token = _tokenService.GenerateAuthToken(user.Email, user.Role);

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    token,
                    email = user.Email,
                    role = user.Role
                },
                "Login exitoso"
            ));
        }


[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(
            ApiResponse<object>.Fail(
                "Datos inválidos.",
                400,
                ModelState
            )
        );
    }

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (user == null)
    {
        return Ok(
            ApiResponse<object>.Ok(
                null,
                "Si el correo existe, enviamos instrucciones."
            )
        );
    }

    var token = Guid.NewGuid().ToString();

    user.ResetToken = token;
    user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

    await _context.SaveChangesAsync();

    var resetLink =
        $"http://localhost:5173/reset-password?token={token}";

    await _emailService.SendPasswordResetEmail(
        dto.Email,
        resetLink
    );

    return Ok(
        ApiResponse<object>.Ok(
            null,
            "Correo enviado correctamente."
        )
    );
}


        [HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(
            ApiResponse<object>.Fail(
                "Datos inválidos.",
                400,
                ModelState
            )
        );
    }

    var user = await _context.Users
        .FirstOrDefaultAsync(u =>
            u.ResetToken == dto.Token);

    if (user == null)
    {
        return BadRequest(
            ApiResponse<object>.Fail(
                "Token inválido.",
                400
            )
        );
    }

    if (user.ResetTokenExpiry < DateTime.UtcNow)
    {
        return BadRequest(
            ApiResponse<object>.Fail(
                "Token expirado.",
                400
            )
        );
    }

    user.PasswordHash =
        BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

    // Inutilizar token después de usarlo
    user.ResetToken = null;
    user.ResetTokenExpiry = null;

    await _context.SaveChangesAsync();

    return Ok(
        ApiResponse<object>.Ok(
            null,
            "Contraseña actualizada correctamente."
        )
    );
}
           
    }
}