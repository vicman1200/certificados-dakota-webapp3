using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Interfaz para el servicio de generación de tokens JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        string GenerateToken(UsuarioInfo usuario);
        
        /// <summary>
        /// Valida un token JWT
        /// </summary>
        bool ValidateToken(string token);
    }
}

