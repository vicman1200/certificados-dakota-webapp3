using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration, ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no está configurada");
        }

        /// <summary>
        /// Valida credenciales de usuario contra la base de datos
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y información del usuario</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="403">Usuario bloqueado o inactivo</response>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Unauthorized(new ErrorResponse
                    {
                        Code = 401,
                        Message = "Usuario y contraseña son requeridos"
                    });
                }

                // Buscar usuario en la base de datos
                var usuario = await ObtenerUsuarioPorNombreAsync(request.Usuario);

                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login con usuario inexistente: {Usuario}", request.Usuario);
                    return Unauthorized(new ErrorResponse
                    {
                        Code = 401,
                        Message = "Usuario o contraseña incorrectos"
                    });
                }

                // Verificar si el usuario está activo
                if (!usuario.Activo)
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {Usuario}", request.Usuario);
                    return StatusCode(403, new ErrorResponse
                    {
                        Code = 403,
                        Message = "Usuario inactivo. Contacte al administrador"
                    });
                }

                // Verificar si el usuario está bloqueado
                if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
                    {
                    _logger.LogWarning("Intento de login con usuario bloqueado: {Usuario}", request.Usuario);
                    return StatusCode(403, new ErrorResponse
                    {
                        Code = 403,
                        Message = $"Usuario bloqueado hasta {usuario.BloqueadoHasta.Value:dd/MM/yyyy HH:mm}"
                    });
                }

                // Verificar contraseña
                bool passwordValida = VerificarPassword(request.Password, usuario.PasswordHash);

                if (!passwordValida)
                {
                    // Incrementar intentos fallidos
                    await IncrementarIntentosFallidosAsync(usuario.Id);

                    _logger.LogWarning("Intento de login con contraseña incorrecta: {Usuario}", request.Usuario);
                    return Unauthorized(new ErrorResponse
                    {
                        Code = 401,
                        Message = "Usuario o contraseña incorrectos"
                    });
                }

                // Login exitoso: resetear intentos fallidos y actualizar último acceso
                await ResetearIntentosFallidosAsync(usuario.Id);
                await ActualizarUltimoAccesoAsync(usuario.Id);

                // Generar token JWT
                var token = GenerarTokenJWT(usuario);

                _logger.LogInformation("Login exitoso para usuario: {Usuario}", request.Usuario);

                return Ok(new LoginResponse
                {
                    Token = token,
                    Usuario = new UsuarioInfo
                    {
                        Usuario = usuario.Usuario,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        Rol = usuario.Rol
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar login para usuario: {Usuario}", request.Usuario);
                return StatusCode(500, new ErrorResponse
                {
                    Code = 500,
                    Message = "Error interno del servidor. Por favor, intente nuevamente"
                });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        private async Task<UsuarioBD?> ObtenerUsuarioPorNombreAsync(string nombreUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    Id, Usuario, PasswordHash, Nombre, Email, Rol, Activo, 
                    FechaCreacion, FechaUltimoAcceso, IntentosFallidos, BloqueadoHasta
                FROM Usuarios
                WHERE Usuario = @Usuario";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Usuario", nombreUsuario);

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new UsuarioBD
                {
                    Id = reader.GetInt32(0),
                    Usuario = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Nombre = reader.GetString(3),
                    Email = reader.GetString(4),
                    Rol = reader.GetString(5),
                    Activo = reader.GetBoolean(6),
                    FechaCreacion = reader.GetDateTime(7),
                    FechaUltimoAcceso = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    IntentosFallidos = reader.GetInt32(9),
                    BloqueadoHasta = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
                };
            }

            return null;
        }

        /// <summary>
        /// Verifica si la contraseña proporcionada coincide con el hash almacenado
        /// </summary>
        private bool VerificarPassword(string password, string passwordHash)
        {
            // Opción 1: Usar BCrypt (Recomendado)
            // Requiere: dotnet add package BCrypt.Net-Next
            // return BCrypt.Net.BCrypt.Verify(password, passwordHash);

            // Opción 2: Usar PBKDF2 (si no puedes usar BCrypt)
            // Este es un ejemplo básico - en producción usa una librería probada
            try
            {
                // Si el hash comienza con $2a$ o $2b$, es BCrypt
                if (passwordHash.StartsWith("$2a$") || passwordHash.StartsWith("$2b$"))
                {
                    // Usar BCrypt.Net-Next
                    return BCrypt.Net.BCrypt.Verify(password, passwordHash);
                }
                
                // Si no, asumir que es un hash simple (NO RECOMENDADO para producción)
                // En producción, siempre usa BCrypt o PBKDF2
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Incrementa el contador de intentos fallidos y bloquea la cuenta si es necesario
        /// </summary>
        private async Task IncrementarIntentosFallidosAsync(int usuarioId)
        {
            const int MAX_INTENTOS = 5;
            const int MINUTOS_BLOQUEO = 30;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE Usuarios 
                SET IntentosFallidos = IntentosFallidos + 1,
                    BloqueadoHasta = CASE 
                        WHEN IntentosFallidos + 1 >= @MaxIntentos 
                        THEN DATEADD(MINUTE, @MinutosBloqueo, GETUTCDATE())
                        ELSE BloqueadoHasta
                    END
                WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", usuarioId);
            command.Parameters.AddWithValue("@MaxIntentos", MAX_INTENTOS);
            command.Parameters.AddWithValue("@MinutosBloqueo", MINUTOS_BLOQUEO);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Resetea el contador de intentos fallidos después de un login exitoso
        /// </summary>
        private async Task ResetearIntentosFallidosAsync(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE Usuarios 
                SET IntentosFallidos = 0,
                    BloqueadoHasta = NULL
                WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", usuarioId);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Actualiza la fecha del último acceso del usuario
        /// </summary>
        private async Task ActualizarUltimoAccesoAsync(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE Usuarios 
                SET FechaUltimoAcceso = GETUTCDATE()
                WHERE Id = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", usuarioId);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Genera un token JWT para el usuario autenticado
        /// </summary>
        private string GenerarTokenJWT(UsuarioBD usuario)
        {
            var secretKey = _configuration["Jwt:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey no está configurada");
            
            var issuer = _configuration["Jwt:Issuer"] ?? "bradial.mx";
            var audience = _configuration["Jwt:Audience"] ?? "bradial.mx";
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.Usuario),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol),
                    new Claim("nombre", usuario.Nombre)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    /// <summary>
    /// Modelo interno para representar un usuario de la base de datos
    /// </summary>
    internal class UsuarioBD
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }
        public int IntentosFallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }
}

