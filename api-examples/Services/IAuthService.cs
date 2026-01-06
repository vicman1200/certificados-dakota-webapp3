using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valida las credenciales del usuario
        /// </summary>
        Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password);
        
        /// <summary>
        /// Obtiene la información completa del usuario
        /// </summary>
        Task<UsuarioInfo?> ObtenerUsuarioAsync(string usuario);
    }
}

