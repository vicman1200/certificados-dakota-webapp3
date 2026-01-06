using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Servicio de autenticación
    /// En producción, esto debería consultar una base de datos
    /// </summary>
    public class AuthService : IAuthService
    {
        // TODO: Reemplazar con acceso a base de datos
        // En producción, usar Entity Framework Core o ADO.NET
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
                        Email = "admin@miapi.com"
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
                        Email = "usuario@miapi.com"
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
                        Email = "demo@miapi.com"
                    }
                )
            }
        };

        public Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password)
        {
            // Validar que el usuario exista
            if (!_usuarios.ContainsKey(usuario))
            {
                return Task.FromResult<UsuarioInfo?>(null);
            }

            var (storedPassword, userInfo) = _usuarios[usuario];

            // TODO: En producción, comparar el hash de la contraseña
            // Ejemplo: BCrypt.Net.BCrypt.Verify(password, storedPassword)
            if (storedPassword != password)
            {
                return Task.FromResult<UsuarioInfo?>(null);
            }

            return Task.FromResult<UsuarioInfo?>(userInfo);
        }

        public Task<UsuarioInfo?> ObtenerUsuarioAsync(string usuario)
        {
            if (_usuarios.ContainsKey(usuario))
            {
                return Task.FromResult<UsuarioInfo?>(_usuarios[usuario].Info);
            }

            return Task.FromResult<UsuarioInfo?>(null);
        }
    }
}

