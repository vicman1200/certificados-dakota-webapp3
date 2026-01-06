# Configuración de HTTPS para Producción

## Problema

Remover `UseHttpsRedirection()` funciona en desarrollo, pero en producción **NECESITAS** HTTPS por seguridad.

## Solución: Configuración Condicional

Configura el middleware para que:
- **En desarrollo**: NO redirija a HTTPS (permite HTTP)
- **En producción**: SÍ redirija a HTTPS (fuerza HTTPS)

## Código Recomendado

En tu `Program.cs`, cambia la sección del pipeline:

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // NO usar HTTPS redirection en desarrollo si trabajas con HTTP
}

// IMPORTANTE: El orden del middleware es CRÍTICO
// Solo usar HTTPS redirection en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowVueApp");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Explicación

### Desarrollo (Development)
- `app.Environment.IsDevelopment()` = `true`
- `UseHttpsRedirection()` NO se ejecuta
- Permite peticiones HTTP directas
- CORS funciona correctamente

### Producción
- `app.Environment.IsDevelopment()` = `false`
- `UseHttpsRedirection()` SÍ se ejecuta
- Todas las peticiones HTTP se redirigen a HTTPS
- Seguridad garantizada

## Configuración del Environment en Producción

### En Azure / IIS
- El environment se configura automáticamente
- No necesitas hacer nada adicional

### En Docker / Linux
```bash
export ASPNETCORE_ENVIRONMENT=Production
```

### En appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Jwt": {
    "SecretKey": "TU_CLAVE_SECRETA_PARA_PRODUCCION",
    "Issuer": "api.tu-dominio.com",
    "Audience": "api.tu-dominio.com",
    "ExpirationMinutes": "30"
  }
}
```

## Verificación

### Desarrollo
```bash
# Verifica que esté en Development
dotnet run
# Debe mostrar: Environment: Development
```

### Producción
```bash
# Ejecuta en modo Production
dotnet run --environment Production
# O configura la variable de entorno
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## Actualizar Frontend para Producción

En `src/services/api.js`, también debes configurar condicionalmente:

```javascript
import axios from 'axios'

// Determinar la URL base según el environment
const getBaseURL = () => {
  // En desarrollo
  if (process.env.NODE_ENV === 'development') {
    return 'http://localhost:5045/api'
  }
  // En producción
  return 'https://api.tu-dominio.com/api'
}

const api = axios.create({
  baseURL: getBaseURL(),
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// ... resto del código
```

O usar variables de entorno de Quasar:

```javascript
import axios from 'axios'

const api = axios.create({
  baseURL: process.env.API_URL || 'http://localhost:5045/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})
```

Y en `quasar.config.js`:
```javascript
build: {
  env: {
    API_URL: process.env.API_URL || 'http://localhost:5045/api'
  }
}
```

## Resumen

✅ **Desarrollo**: Sin HTTPS redirection (permite HTTP)  
✅ **Producción**: Con HTTPS redirection (fuerza HTTPS)  
✅ **Seguridad**: Garantizada en producción  
✅ **Desarrollo**: Sin problemas de CORS

