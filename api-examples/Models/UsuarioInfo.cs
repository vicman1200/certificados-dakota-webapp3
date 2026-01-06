namespace bradial_webapi.Models
{
    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public class UsuarioInfo
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
