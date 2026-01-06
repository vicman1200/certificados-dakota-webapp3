# PASO 7: Crear el Controlador de Login

## Objetivo
Crear el controlador `LoginController` que manejará las peticiones de autenticación en el endpoint `/api/login`.

## Paso 7.1: Crear el archivo LoginController.cs

Crea un nuevo archivo llamado `LoginController.cs` dentro de la carpeta `Controllers`.

### Opción A: Desde Visual Studio
1. Haz clic derecho en la carpeta `Controllers`
2. Selecciona **Add** → **Controller**
3. Selecciona **API Controller - Empty**
4. Nombra el controlador: `LoginController`
5. Reemplaza el contenido con el código que te proporcionaré

### Opción B: Crear manualmente
Crea el archivo `Controllers/LoginController.cs` y copia el código.

---

## Paso 7.2: Código del Controlador (Versión Básica)

Esta es una versión básica que funciona sin servicios personalizados. Más adelante la mejoraremos.

### Código completo para LoginController.cs:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using bradial_webapi.Models;

namespace bradial_webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginController> _logger;

        // Usuarios de prueba (en producción, esto vendría de una base de datos)
        private readonly Dictionary<string, (string Password, UsuarioInfo Info)> _usuarios = new()
        {
            {
                "admin",
                (
                    Password: "admin123",
                    Info: new UsuarioInfo
                    {
                        Usuario = "admin",
                        Nombre = "Administrador",
                        Rol = "Administrador",
                        Email = "admin@bradial.mx"
                    }
                )
            },
            {
                "usuario",
                (
                    Password: "password123",
                    Info: new UsuarioInfo
                    {
                        Usuario = "usuario",
                        Nombre = "Usuario Demo",
                        Rol = "Usuario",
                        Email = "usuario@bradial.mx"
                    }
                )
            },
            {
                "demo",
                (
                    Password: "demo123",
                    Info: new UsuarioInfo
                    {
                        Usuario = "demo",
                        Nombre = "Usuario de Prueba",
                        Rol = "Usuario",
                        Email = "demo@bradial.mx"
                    }
                )
            }
        };

        public LoginController(IConfiguration configuration, ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Inicia sesión con credenciales de usuario
        /// </summary>
        /// <param name="request">Credenciales de acceso</param>
        /// <returns>Token JWT y información del usuario</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">Credenciales incorrectas</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errores = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                        .ToList();

                    return BadRequest(new ErrorResponse
                    {
                        Mensaje = "Datos de entrada inválidos",
                        Detalle = string.Join(", ", errores),
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Validar credenciales
                if (!_usuarios.ContainsKey(request.Usuario))
                {
                    _logger.LogWarning("Intento de login fallido para el usuario: {Usuario}", request.Usuario);
                    return Unauthorized(new ErrorResponse
                    {
                        Mensaje = "Credenciales inválidas",
                        StatusCode = StatusCodes.Status401Unauthorized
                    });
                }

                var (storedPassword, userInfo) = _usuarios[request.Usuario];

                // Verificar contraseña (en producción, esto debería usar hash)
                if (storedPassword != request.Password)
                {
                    _logger.LogWarning("Intento de login fallido para el usuario: {Usuario}", request.Usuario);
                    return Unauthorized(new ErrorResponse
                    {
                        Mensaje = "Credenciales inválidas",
                        StatusCode = StatusCodes.Status401Unauthorized
                    });
                }

                // Generar token JWT
                var token = GenerateJwtToken(userInfo);

                _logger.LogInformation("Login exitoso para el usuario: {Usuario}", request.Usuario);

                // Retornar respuesta exitosa
                var response = new LoginResponse
                {
                    Token = token,
                    Usuario = userInfo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el login para el usuario: {Usuario}", request.Usuario);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Mensaje = "Error interno del servidor",
                    Detalle = ex.Message,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Genera un token JWT para el usuario autenticado
        /// </summary>
        private string GenerateJwtToken(UsuarioInfo usuario)
        {
            // Obtener configuración JWT
            var secretKey = _configuration["Jwt:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey no está configurada");
            var issuer = _configuration["Jwt:Issuer"] ?? "bradial.mx";
            var audience = _configuration["Jwt:Audience"] ?? "bradial.mx";
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear claims (información en el token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Usuario),
                new Claim(ClaimTypes.NameIdentifier, usuario.Usuario),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("nombre", usuario.Nombre)
            };

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            // Retornar el token como string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```

---

## Paso 7.3: Verificar que Compila

Después de crear el archivo, ejecuta:

```bash
dotnet build
```

Si compila sin errores, el controlador está listo. ✅

---

## Paso 7.4: Probar el Endpoint

1. Ejecuta la aplicación:
```bash
dotnet run
```

2. Abre tu navegador y ve a:
   - Swagger UI: `https://localhost:7254` (o el puerto que te indique)
   - O directamente: `https://localhost:7254/swagger`

3. Busca el endpoint `POST /api/login` y prueba con:
   - Usuario: `admin`
   - Password: `admin123`

---

## ¿Qué hace este controlador?

1. **Recibe** las credenciales (usuario y contraseña) en formato JSON
2. **Valida** que los datos sean correctos (no vacíos, longitud mínima)
3. **Verifica** las credenciales contra una lista de usuarios (por ahora en memoria)
4. **Genera** un token JWT si las credenciales son válidas
5. **Retorna** el token y la información del usuario

---

## Usuarios de Prueba Disponibles

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| admin | admin123 | Administrador |
| usuario | password123 | Usuario |
| demo | demo123 | Usuario |

---

## Próximo Paso

Una vez que el controlador funcione correctamente, podremos:
- Crear servicios personalizados para separar la lógica
- Mejorar la seguridad
- Conectar con una base de datos

