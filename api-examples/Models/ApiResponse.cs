namespace WebApi.Models
{
    /// <summary>
    /// Modelo genérico para respuestas de la API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errores { get; set; } = new();
    }

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

