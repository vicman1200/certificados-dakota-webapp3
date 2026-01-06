# Implementación de Generación de PDFs

## Resumen

Esta solución implementa la generación de PDFs en el **backend** usando **QuestPDF**, que es una librería moderna y fácil de usar para .NET.

## Ventajas de Generar PDFs en el Backend

✅ **Seguridad**: Los datos sensibles no se exponen al cliente  
✅ **Rendimiento**: El servidor tiene más recursos para procesar  
✅ **Consistencia**: Mismo formato en todos los clientes  
✅ **Mantenibilidad**: Cambios de plantilla sin actualizar frontend  

---

## Pasos de Implementación

### Paso 1: Instalar QuestPDF

En la terminal del proyecto de la Web API, ejecuta:

```bash
dotnet add package QuestPDF
```

O desde Visual Studio:
1. Click derecho en el proyecto → **Manage NuGet Packages**
2. Buscar "QuestPDF"
3. Instalar la versión más reciente (2024.3.10 o superior)

**Nota**: El archivo `WebApi.csproj` ya está actualizado con este paquete.

---

### Paso 2: Copiar los Archivos al Proyecto de la API

Copia los siguientes archivos a tu proyecto de Web API (`C:\Users\atack\source\repos\bradial-webapi\`):

1. **`Services/IPdfService.cs`** → Copiar a `Services/IPdfService.cs`
2. **`Services/PdfService.cs`** → Copiar a `Services/PdfService.cs`
3. **`Models/Certificado.cs`** → Copiar a `Models/Certificado.cs`
4. **`Controllers/CertificadoController.cs`** → Copiar a `Controllers/CertificadoController.cs`

---

### Paso 3: Registrar el Servicio en Program.cs

Abre `Program.cs` y agrega esta línea **después** de `builder.Services.AddAuthorization();`:

```csharp
// Registrar el servicio de generación de PDFs
builder.Services.AddScoped<IPdfService, PdfService>();
```

**Importante**: Agrega el `using` al inicio del archivo:

```csharp
using bradial_webapi.Services;
```

---

### Paso 4: Actualizar el Controlador (Obtener Certificado de BD)

En `CertificadoController.cs`, reemplaza la sección de datos de ejemplo con una consulta real a tu base de datos:

```csharp
// TODO: Reemplazar con consulta real a la base de datos
var certificado = await _certificadoRepository.ObtenerPorIdAsync(uid);
if (certificado == null) 
{
    return NotFound(new ErrorResponse 
    { 
        Code = 404, 
        Message = "Certificado no encontrado" 
    });
}
```

---

### Paso 5: Probar el Endpoint

1. Ejecuta la Web API
2. Abre Swagger: `https://localhost:7254` o `http://localhost:5045`
3. Busca el endpoint `GET /api/certificado/{uid}/pdf`
4. Autentícate primero (usa el endpoint `/api/login`)
5. Prueba descargar un PDF con un `uid` válido

---

### Paso 6: Frontend ya está Listo

El frontend ya está configurado para consumir el endpoint:

- ✅ `src/services/certificadoService.js` tiene la función `descargarPdf()`
- ✅ `src/pages/DashboardPage.vue` tiene la función `descargarCertificado()` implementada
- ✅ Al hacer clic en el icono de PDF en la tabla, se descargará automáticamente

---

## Personalizar la Plantilla del PDF

Para modificar el diseño del PDF, edita el archivo `Services/PdfService.cs`. QuestPDF usa un sistema de composición fluida muy intuitivo.

### Ejemplo: Cambiar Colores

```csharp
.Color("#ff8000")  // Color naranja de Bradial
```

### Ejemplo: Agregar Logo

```csharp
page.Header()
    .Row(row =>
    {
        row.RelativeItem().Image("wwwroot/images/logo.png")
            .Width(100);
        // ... resto del header
    });
```

### Documentación de QuestPDF

- 📚 [Documentación oficial](https://www.questpdf.com/)
- 🎨 [Ejemplos de diseño](https://www.questpdf.com/documentation/getting-started.html)

---

## Estructura del PDF Generado

El PDF incluye:

1. **Header**: Logo/Título de Bradial + Número de Certificado
2. **Información del Titular**: Nombre completo
3. **Información del Vehículo**: Marca, Submarca, Modelo, No. Serie
4. **Vigencia**: Fechas de expedición, vigencia desde/hasta, años de vigencia
5. **Estado**: Estado actual del certificado
6. **Footer**: Fecha de generación y usuario que lo creó

---

## Solución de Problemas

### Error: "QuestPDF.Settings.License is not set"

Asegúrate de que en `PdfService.cs` esté esta línea al inicio del método:

```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

### Error: "Certificado no encontrado"

Verifica que:
1. El `uid` existe en la base de datos
2. El controlador está consultando correctamente la BD
3. El usuario está autenticado (el endpoint requiere `[Authorize]`)

### El PDF no se descarga en el navegador

Verifica que:
1. El `responseType: 'blob'` esté configurado en `certificadoService.js`
2. El endpoint retorna `File(pdfBytes, "application/pdf", ...)`
3. No hay errores de CORS

---

## Próximos Pasos (Opcional)

1. **Guardar PDFs en el servidor**: Modifica el controlador para guardar el PDF generado en una carpeta y actualizar `RutaArchivoPDF` en la BD
2. **Plantillas personalizadas**: Crea diferentes plantillas según el tipo de certificado
3. **Firmas digitales**: Agrega firma digital al PDF usando QuestPDF
4. **Watermarks**: Agrega marcas de agua para documentos confidenciales

---

## Archivos Creados/Modificados

### Backend (Web API)
- ✅ `Services/IPdfService.cs` (nuevo)
- ✅ `Services/PdfService.cs` (nuevo)
- ✅ `Models/Certificado.cs` (nuevo)
- ✅ `Controllers/CertificadoController.cs` (nuevo)
- ✅ `WebApi.csproj` (actualizado con QuestPDF)
- ⚠️ `Program.cs` (necesita agregar registro del servicio)

### Frontend (Quasar)
- ✅ `src/services/certificadoService.js` (actualizado con `descargarPdf()`)
- ✅ `src/pages/DashboardPage.vue` (actualizado con descarga funcional)

---

¡Listo! Ya puedes generar y descargar PDFs de certificados desde el dashboard. 🎉


