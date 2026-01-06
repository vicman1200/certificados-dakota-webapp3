namespace bradial_webapi.Models
{
    /// <summary>
    /// Modelo para la respuesta del login
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioInfo Usuario { get; set; } = new UsuarioInfo();
    }
}
