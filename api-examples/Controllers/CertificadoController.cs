using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using bradial_webapi.Models;
using bradial_webapi.Services;

namespace bradial_webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación
    [Produces("application/json")]
    public class CertificadoController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<CertificadoController> _logger;

        public CertificadoController(IPdfService pdfService, ILogger<CertificadoController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        /// <summary>
        /// Descarga el PDF de un certificado
        /// </summary>
        /// <param name="uid">ID del certificado</param>
        /// <returns>Archivo PDF</returns>
        /// <response code="200">PDF generado correctamente</response>
        /// <response code="404">Certificado no encontrado</response>
        /// <response code="401">No autenticado</response>
        [HttpGet("{uid}/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult DescargarPdf(int uid)
        {
            try
            {
                // TODO: Obtener el certificado de la base de datos usando el uid
                // Por ahora, usamos datos de ejemplo para demostración
                // En producción, deberías hacer algo como:
                // var certificado = await _certificadoRepository.ObtenerPorIdAsync(uid);
                // if (certificado == null) return NotFound();

                // Datos de ejemplo (reemplazar con consulta a BD)
                var certificado = new Certificado
                {
                    Id = uid,
                    NoCertificado = $"BRBBVA{uid:D6}",
                    Titular = "Ejemplo Titular",
                    FechaExpedicion = DateTime.Now,
                    AniosVigencia = 1,
                    VigenteDesde = DateTime.Now,
                    VigenteHasta = DateTime.Now.AddYears(1),
                    Marca = "Ejemplo Marca",
                    Submarca = "Ejemplo Submarca",
                    Modelo = "2025",
                    NumeroSerie = "SERIE123456",
                    Estado = "Vigente",
                    CreadoPor = User.Identity?.Name ?? "Usuario",
                    FechaCreacion = DateTime.Now
                };

                // Generar el PDF
                var pdfBytes = _pdfService.GenerarCertificadoPdf(certificado);

                _logger.LogInformation("PDF generado para certificado {Uid} por usuario {Usuario}", uid, User.Identity?.Name);

                // Retornar el archivo PDF
                return File(pdfBytes, "application/pdf", $"Certificado_{certificado.NoCertificado}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF para certificado {Uid}", uid);
                return StatusCode(500, new ErrorResponse
                {
                    Code = 500,
                    Message = "Error al generar el PDF del certificado"
                });
            }
        }
    }
}


