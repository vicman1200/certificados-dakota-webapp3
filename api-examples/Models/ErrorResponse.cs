namespace bradial_webapi.Models
{
    /// <summary>
    /// Modelo para respuestas de error
    /// </summary>
    public class ErrorResponse
    {
        public string Mensaje { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public int StatusCode { get; set; }
    }
}
