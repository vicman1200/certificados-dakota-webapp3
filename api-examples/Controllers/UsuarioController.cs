using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using bradial_webapi.Models;

namespace bradial_webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la información del usuario actual autenticado
        /// </summary>
        /// <returns>Información del usuario</returns>
        /// <response code="200">Usuario autenticado</response>
        /// <response code="401">No autenticado</response>
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(UsuarioInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetPerfil()
        {
            try
            {
                // Obtener información del usuario desde los claims del token
                var usuario = User.Identity?.Name;
                var nombre = User.FindFirst("nombre")?.Value ?? usuario ?? "Usuario";
                var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Usuario";
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

                var usuarioInfo = new UsuarioInfo
                {
                    Usuario = usuario ?? string.Empty,
                    Nombre = nombre,
                    Rol = rol,
                    Email = email
                };

                _logger.LogInformation("Usuario {Usuario} consultó su perfil", usuario);

                return Ok(usuarioInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Mensaje = "Error al obtener el perfil",
                    Detalle = ex.Message,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Verifica si el token es válido
        /// </summary>
        /// <returns>Estado del token</returns>
        [HttpGet("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult VerifyToken()
        {
            var usuario = User.Identity?.Name;
            
            return Ok(new
            {
                mensaje = "Token válido",
                usuario = usuario,
                autenticado = User.Identity?.IsAuthenticated ?? false,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}

