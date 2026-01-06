# Generación de PDFs en el Backend

## Paso 1: Instalar QuestPDF

Ejecuta en la terminal del proyecto de la Web API:

```bash
dotnet add package QuestPDF
```

O desde Visual Studio:
1. Click derecho en el proyecto → **Manage NuGet Packages**
2. Buscar "QuestPDF"
3. Instalar la versión más reciente

---

## Paso 2: Actualizar WebApi.csproj

Agrega esta línea en el `<ItemGroup>` de paquetes:

```xml
<PackageReference Include="QuestPDF" Version="2024.3.10" />
```

---

## Paso 3: Crear el Servicio de Generación de PDF

Crea el archivo `Services/IPdfService.cs`:

```csharp
using bradial_webapi.Models;

namespace bradial_webapi.Services
{
    public interface IPdfService
    {
        byte[] GenerarCertificadoPdf(Certificado certificado);
    }
}
```

---

## Paso 4: Implementar el Servicio PDF

Crea el archivo `Services/PdfService.cs` con la implementación completa.

---

## Paso 5: Registrar el Servicio en Program.cs

Agrega esta línea después de `builder.Services.AddAuthorization();`:

```csharp
builder.Services.AddScoped<IPdfService, PdfService>();
```

---

## Paso 6: Crear el Controlador de Certificados

Crea el archivo `Controllers/CertificadoController.cs` con el endpoint para descargar PDFs.

---

## Paso 7: Actualizar el Frontend

Actualiza `src/services/certificadoService.js` y `src/pages/DashboardPage.vue` para consumir el endpoint de descarga.


