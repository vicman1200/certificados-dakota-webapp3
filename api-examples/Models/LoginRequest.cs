using System.ComponentModel.DataAnnotations;

namespace bradial_webapi.Models
{
    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        [MinLength(3, ErrorMessage = "El usuario debe tener al menos 3 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
