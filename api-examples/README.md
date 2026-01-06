# Web API - Autenticación con JWT

API REST desarrollada en C# (.NET 8) para autenticación de usuarios usando JWT (JSON Web Tokens).

## 📦 Paquetes NuGet Requeridos

Los siguientes paquetes NuGet están incluidos en el archivo `WebApi.csproj`:

### Autenticación JWT
- **Microsoft.AspNetCore.Authentication.JwtBearer** (v8.0.0)
  - Proporciona middleware para autenticación basada en tokens JWT
  
- **System.IdentityModel.Tokens.Jwt** (v7.0.3)
  - Biblioteca para crear, validar y leer tokens JWT

### Documentación API
- **Swashbuckle.AspNetCore** (v6.5.0)
  - Genera documentación Swagger/OpenAPI automática

### Entity Framework Core (Opcional)
- **Microsoft.EntityFrameworkCore** (v8.0.0)
- **Microsoft.EntityFrameworkCore.SqlServer** (v8.0.0)
- **Microsoft.EntityFrameworkCore.Tools** (v8.0.0)

*Nota: Estos paquetes están incluidos para cuando implementes acceso a base de datos. Por ahora no son necesarios.*

## 🚀 Instalación y Configuración

### Requisitos Previos
- .NET SDK 8.0 o superior
- Visual Studio 2022, VS Code, o JetBrains Rider

### Pasos de Instalación

1. **Crear el proyecto** (si no existe):
```bash
dotnet new webapi -n WebApi
cd WebApi
```

2. **Instalar paquetes NuGet**:
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

O simplemente restaurar los paquetes:
```bash
dotnet restore
```

3. **Copiar los archivos** del proyecto:
   - `Program.cs` → raíz del proyecto
   - `Controllers/LoginController.cs` → Carpeta Controllers
   - `Models/*.cs` → Carpeta Models
   - `Services/*.cs` → Carpeta Services
   - `appsettings.json` → raíz del proyecto

4. **Configurar la clave secreta JWT**:
   
   Edita `appsettings.json` y cambia `Jwt:SecretKey` por una clave segura (mínimo 32 caracteres):

```json
{
  "Jwt": {
    "SecretKey": "TU_CLAVE_SECRETA_SUPER_SEGURA_AQUI_MINIMO_32_CARACTERES",
    "Issuer": "miapi.com",
    "Audience": "miapi.com",
    "ExpirationMinutes": "60"
  }
}
```

5. **Configurar CORS**:
   
   En `Program.cs`, ajusta los orígenes permitidos según tu frontend:

```csharp
policy.WithOrigins(
    "http://localhost:9000",  // Quasar por defecto
    "https://tu-dominio.com"  // Tu dominio en producción
)
```

## 📝 Endpoints Disponibles

### POST /api/login

Inicia sesión con credenciales de usuario.

**Request Body:**
```json
{
  "usuario": "admin",
  "password": "admin123"
}
```

**Response 200 OK:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "usuario": "admin",
    "nombre": "Administrador",
    "rol": "Administrador",
    "email": "admin@miapi.com"
  }
}
```

**Response 401 Unauthorized:**
```json
{
  "mensaje": "Credenciales inválidas",
  "statusCode": 401
}
```

### POST /api/login/verify

Verifica si un token JWT es válido (requiere autenticación).

**Headers:**
```
Authorization: Bearer {token}
```

**Response 200 OK:**
```json
{
  "mensaje": "Token válido",
  "usuario": "admin",
  "claims": [...]
}
```

## 👤 Usuarios de Prueba

El servicio incluye los siguientes usuarios de prueba:

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| admin | admin123 | Administrador |
| usuario | password123 | Usuario |
| demo | demo123 | Usuario |

⚠️ **IMPORTANTE**: Estos son solo para desarrollo. En producción, debes implementar una base de datos.

## 🔧 Ejecutar la Aplicación

### Desarrollo
```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger` o `https://localhost:5001/swagger`

### Producción
```bash
dotnet publish -c Release
```

## 🔐 Seguridad

### Configuración de Producción

1. **Cambiar la clave secreta JWT**:
   - Genera una clave aleatoria segura (mínimo 32 caracteres)
   - Usa variables de entorno o Azure Key Vault

2. **Habilitar HTTPS**:
   - En `Program.cs`, cambiar `RequireHttpsMetadata` a `true`
   - Configurar certificados SSL

3. **Implementar hash de contraseñas**:
   - Usar BCrypt.Net o similar
   - Nunca almacenar contraseñas en texto plano

4. **Rate Limiting**:
   - Implementar límites de intentos de login
   - Prevenir ataques de fuerza bruta

### Recomendaciones Adicionales

- ✅ Usar refresh tokens para tokens de larga duración
- ✅ Implementar logging de intentos de login fallidos
- ✅ Validar y sanitizar todas las entradas
- ✅ Usar HTTPS en producción
- ✅ Implementar CORS de forma restrictiva
- ✅ Rotar las claves JWT periódicamente

## 🗄️ Integración con Base de Datos

Para conectar con una base de datos (SQL Server, PostgreSQL, etc.):

1. **Instalar Entity Framework Core** (si no está instalado)
2. **Crear DbContext**:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; }
    
    // Configuración...
}
```

3. **Modificar AuthService** para consultar la base de datos:
```csharp
public async Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password)
{
    var usuarioDb = await _context.Usuarios
        .FirstOrDefaultAsync(u => u.Usuario == usuario);
    
    if (usuarioDb == null) return null;
    
    // Verificar hash de contraseña
    if (!BCrypt.Net.BCrypt.Verify(password, usuarioDb.PasswordHash))
        return null;
    
    return new UsuarioInfo { /* mapear datos */ };
}
```

## 📚 Estructura del Proyecto

```
WebApi/
├── Controllers/
│   └── LoginController.cs      # Controlador de autenticación
├── Models/
│   ├── LoginRequest.cs         # Modelo de solicitud
│   ├── LoginResponse.cs        # Modelo de respuesta
│   ├── UsuarioInfo.cs          # Información del usuario
│   └── ApiResponse.cs          # Respuestas genéricas
├── Services/
│   ├── IAuthService.cs         # Interfaz de autenticación
│   ├── AuthService.cs          # Servicio de autenticación
│   ├── IJwtService.cs          # Interfaz de JWT
│   └── JwtService.cs           # Servicio de JWT
├── Program.cs                  # Configuración de la aplicación
├── appsettings.json            # Configuración
└── WebApi.csproj               # Archivo del proyecto
```

## 🐛 Troubleshooting

### Error: "JWT SecretKey no está configurada"
- Verifica que `appsettings.json` tenga la sección `Jwt:SecretKey`

### Error de CORS
- Asegúrate de que la URL del frontend esté en la lista de orígenes permitidos en `Program.cs`

### Token inválido
- Verifica que el `Issuer` y `Audience` coincidan entre la API y el frontend
- Asegúrate de que el token no haya expirado

## 📞 Soporte

Para más información sobre JWT en .NET:
- [Documentación oficial de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [JWT.io](https://jwt.io/) - Para decodificar y verificar tokens
