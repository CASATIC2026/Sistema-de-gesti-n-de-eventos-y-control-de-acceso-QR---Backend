using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EventAccessControl.API.Services
{
    /// <summary>
    /// Servicio para generar y validar tokens JWT para los tickets de acceso al evento.  
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor que recibe la configuración de la aplicación para obtener los parámetros necesarios para generar 
        /// y validar tokens JWT, como el secreto de firma, emisor, y público.
        /// </summary>
        /// <param name="config"></param>
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para generar un token JWT para un ticket específico. Recibe el identificador del ticket, el 
        /// identificador del evento, y el correo electrónico del usuario asociado al ticket. 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="eventId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GenerateTicketToken(Guid ticketId, Guid eventId, string email)
        {
            var secret = _config["JwtTicket:Secret"] ?? throw new InvalidOperationException("JwtTicket:Secret not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("ticketId", ticketId.ToString()),
                new Claim("eventId", eventId.ToString()),
                new Claim("email", email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtTicket:Issuer"],
                audience: _config["JwtTicket:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Método para validar un token JWT recibido durante la entrada al evento. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var secret = _config["JwtTicket:Secret"] ?? throw new InvalidOperationException("JwtTicket:Secret not configured");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = _config["JwtTicket:Issuer"],
                    ValidAudience = _config["JwtTicket:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Método para generar un token JWT para un usuario específico. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GenerateAuthToken(Guid userId, string email, string role)
        {
            var secret = _config["JwtAuthToken:Secret"]
                ?? throw new InvalidOperationException("JwtAuthToken:Secret not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
               new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
               new Claim(ClaimTypes.Email, email),
               new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtAuthToken:Issuer"],
                audience: _config["JwtAuthToken:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
