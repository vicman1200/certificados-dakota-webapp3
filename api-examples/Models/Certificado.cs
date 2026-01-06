namespace bradial_webapi.Models
{
    /// <summary>
    /// Modelo para representar un certificado
    /// </summary>
    public class Certificado
    {
        public int Id { get; set; }
        public string NoCertificado { get; set; } = string.Empty;
        public string Titular { get; set; } = string.Empty;
        public DateTime FechaExpedicion { get; set; }
        public int AniosVigencia { get; set; }
        public DateTime VigenteDesde { get; set; }
        public DateTime VigenteHasta { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Submarca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string Estado { get; set; } = "Solicitado";
        public string Usuario { get; set; } = string.Empty;
        public string CreadoPor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? Observaciones { get; set; }
        public string? RutaArchivoPDF { get; set; }
    }
}


