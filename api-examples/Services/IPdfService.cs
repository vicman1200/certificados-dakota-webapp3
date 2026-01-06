using bradial_webapi.Models;

namespace bradial_webapi.Services
{
    public interface IPdfService
    {
        byte[] GenerarCertificadoPdf(Certificado certificado);
    }
}


