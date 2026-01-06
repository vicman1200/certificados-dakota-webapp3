using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using bradial_webapi.Models;

namespace bradial_webapi.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerarCertificadoPdf(Certificado certificado)
        {
            // Configurar la licencia de QuestPDF (gratuita para proyectos no comerciales)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("BRADIAL")
                                    .FontSize(24)
                                    .Bold()
                                    .Color("#ff8000");

                                column.Item().Text("Certificado de Garantía")
                                    .FontSize(18)
                                    .Bold()
                                    .Color(Colors.Grey.Darken2);
                            });

                            row.ConstantItem(100).AlignRight().Text($"No. {certificado.NoCertificado}")
                                .FontSize(12)
                                .Bold()
                                .Color(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Información del Titular
                            column.Item().Background(Colors.Grey.Lighten3)
                                .Padding(15)
                                .Column(col =>
                                {
                                    col.Item().Text("INFORMACIÓN DEL TITULAR")
                                        .FontSize(14)
                                        .Bold()
                                        .Color("#ff8000");

                                    col.Item().PaddingTop(5).Text($"Nombre: {certificado.Titular}")
                                        .FontSize(11);
                                });

                            // Información del Vehículo
                            column.Item().Background(Colors.Grey.Lighten4)
                                .Padding(15)
                                .Column(col =>
                                {
                                    col.Item().Text("INFORMACIÓN DEL VEHÍCULO")
                                        .FontSize(14)
                                        .Bold()
                                        .Color("#ff8000");

                                    col.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Marca: {certificado.Marca}");
                                        row.RelativeItem().Text($"Submarca: {certificado.Submarca}");
                                    });

                                    col.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Modelo: {certificado.Modelo}");
                                        row.RelativeItem().Text($"No. Serie: {certificado.NumeroSerie}");
                                    });
                                });

                            // Información de Vigencia
                            column.Item().Background(Colors.Grey.Lighten3)
                                .Padding(15)
                                .Column(col =>
                                {
                                    col.Item().Text("VIGENCIA DEL CERTIFICADO")
                                        .FontSize(14)
                                        .Bold()
                                        .Color("#ff8000");

                                    col.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Fecha de Expedición: {certificado.FechaExpedicion:dd/MM/yyyy}");
                                        row.RelativeItem().Text($"Años de Vigencia: {certificado.AniosVigencia}");
                                    });

                                    col.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Vigente Desde: {certificado.VigenteDesde:dd/MM/yyyy}");
                                        row.RelativeItem().Text($"Vigente Hasta: {certificado.VigenteHasta:dd/MM/yyyy}");
                                    });
                                });

                            // Estado
                            column.Item().AlignCenter()
                                .Padding(10)
                                .Background(Colors.Green.Lighten4)
                                .Text($"Estado: {certificado.Estado}")
                                .FontSize(12)
                                .Bold()
                                .Color(Colors.Green.Darken3);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generado el ");
                            x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").Bold();
                            x.Span(" por ");
                            x.Span(certificado.CreadoPor).Bold();
                        })
                        .FontSize(8)
                        .Color(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}


