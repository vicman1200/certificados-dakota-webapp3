using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar CORS para permitir peticiones desde el frontend Vue.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:9000",      // Puerto por defecto de Quasar en desarrollo
                "http://localhost:8080",      // Puerto alternativo de Vue
                "https://localhost:9000",     // HTTPS en desarrollo
                "https://localhost:8080"      // HTTPS alternativo
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        
        // Para producción, agregar tu dominio real:
        // policy.WithOrigins("https://tu-dominio.com")
        //       .AllowAnyHeader()
        //       .AllowAnyMethod()
        //       .AllowCredentials();
    });
});

// Configurar autenticación JWT
var secretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? (builder.Environment.IsDevelopment() 
        ? "MiClaveSecretaSuperSeguraParaJWT12345678901234567890" 
        : throw new InvalidOperationException("JWT SecretKey no está configurada en appsettings.json"));

var issuer = builder.Configuration["Jwt:Issuer"] ?? "miapi.com";
var audience = builder.Configuration["Jwt:Audience"] ?? "miapi.com";

// Validar que secretKey no sea null antes de convertir a bytes
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey no puede ser nulo o vacío. Verifica appsettings.json o appsettings.Development.json");
}

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Eliminar la tolerancia de tiempo para tokens expirados
    };
    
    // Manejo de eventos para debugging (opcional)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configurar Swagger/OpenAPI con soporte para JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Web API - Autenticación", 
        Version = "v1",
        Description = "API para autenticación con JWT"
    });
    
    // Configurar Swagger para usar JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1");
        c.RoutePrefix = string.Empty; // Swagger UI disponible en la raíz
    });
}

app.UseHttpsRedirection();

// IMPORTANTE: CORS debe ir antes de Authentication y Authorization
app.UseCors("AllowVueApp");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
