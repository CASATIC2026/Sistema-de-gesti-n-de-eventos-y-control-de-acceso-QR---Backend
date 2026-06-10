using EventAccessControl.API.Data;
using EventAccessControl.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community; // Set the license type

// mensajes de error personalizados JSON
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors
                        .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                            ? e.Exception?.Message
                            : e.ErrorMessage)
                        .Where(m => m != null)
                        .ToArray()!
                );

            var response = ApiResponse<object>.Fail(
                "Los datos enviados no son válidos.",
                400,
                errors
            );

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<QRService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    // Add JWT Bearer support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSection = builder.Configuration.GetSection("JwtAuthToken");

var secret = jwtSection["Secret"]
    ?? throw new Exception("JwtAuthToken:Secret no configurado");

var issuer = jwtSection["Issuer"]
    ?? throw new Exception("JwtAuthToken:Issuer no configurado");

var audience = jwtSection["Audience"]
    ?? throw new Exception("JwtAuthToken:Audience no configurado");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true, // importante

        ValidIssuer = issuer,
        ValidAudience = audience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret)
        ),

        ClockSkew = TimeSpan.Zero // evita retrasos en expiración
    };
});

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // policy
        //     .WithOrigins("http://localhost:5173", "https://localhost:5173")
        //     .AllowAnyHeader()
        //     .AllowAnyMethod();

            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});
builder.Services.AddRateLimiter(options =>
{
    // Devolver un 429 Too Many Requests cuando se exceda el límite (estándar HTTP)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Crear una política específica para el registro de tickets
    options.AddPolicy("RegisterTicketPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
            // Agrupamos el límite por la dirección IP del cliente
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3, // Máximo 3 peticiones...
                Window = TimeSpan.FromMinutes(1), // ...por cada 1 minuto
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // Si se pasan de 3, rechazamos inmediatamente sin encolar
            }));
});

//builder.WebHost.UseUrls("http://0.0.0.0:5255");
//builder.WebHost.UseUrls("http://0.0.0.0:5255");
var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();