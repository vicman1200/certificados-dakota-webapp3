# PASO 6: Crear los Modelos para el Login

## Objetivo
Crear los modelos (clases) que representarán las solicitudes y respuestas del endpoint de login.

## Modelos a Crear

Necesitamos crear 3 modelos:

1. **LoginRequest** - Modelo para la solicitud de login (usuario y contraseña)
2. **LoginResponse** - Modelo para la respuesta exitosa del login (token y usuario)
3. **UsuarioInfo** - Modelo para la información del usuario

---

## Paso 6.1: Crear la carpeta Models

Primero, necesitamos crear la carpeta `Models` en la raíz del proyecto (si no existe).

### Opción A: Desde Visual Studio
1. Haz clic derecho en el proyecto en el **Solution Explorer**
2. Selecciona **Add** → **New Folder**
3. Nombra la carpeta: `Models`

### Opción B: Desde la terminal
```bash
mkdir Models
```

O si estás en Windows PowerShell:
```powershell
New-Item -ItemType Directory -Path Models
```

---

## Paso 6.2: Crear el modelo LoginRequest

Crea un nuevo archivo llamado `LoginRequest.cs` dentro de la carpeta `Models`.

### Opción A: Desde Visual Studio
1. Haz clic derecho en la carpeta `Models`
2. Selecciona **Add** → **Class**
3. Nombra la clase: `LoginRequest`
4. Reemplaza el contenido con el código que te proporcionaré

### Opción B: Crear manualmente
Crea el archivo `Models/LoginRequest.cs` y copia el código.

### Código para LoginRequest.cs:

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        [MinLength(3, ErrorMessage = "El usuario debe tener al menos 3 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
```

**Explicación:**
- `[Required]` - Valida que el campo no esté vacío
- `[MinLength]` - Valida la longitud mínima
- `string.Empty` - Inicializa las propiedades con cadena vacía (buena práctica en C#)

---

## Paso 6.3: Crear el modelo UsuarioInfo

Crea un nuevo archivo llamado `UsuarioInfo.cs` dentro de la carpeta `Models`.

### Código para UsuarioInfo.cs:

```csharp
namespace WebApi.Models
{
    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public class UsuarioInfo
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
```

---

## Paso 6.4: Crear el modelo LoginResponse

Crea un nuevo archivo llamado `LoginResponse.cs` dentro de la carpeta `Models`.

### Código para LoginResponse.cs:

```csharp
namespace WebApi.Models
{
    /// <summary>
    /// Modelo para la respuesta del login
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioInfo Usuario { get; set; } = new UsuarioInfo();
    }
}
```

**Explicación:**
- `Token` - El token JWT que se retornará al cliente
- `Usuario` - La información del usuario autenticado

---

## Paso 6.5: Crear el modelo ErrorResponse (Opcional pero recomendado)

Crea un nuevo archivo llamado `ErrorResponse.cs` dentro de la carpeta `Models`.

### Código para ErrorResponse.cs:

```csharp
namespace WebApi.Models
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
```

Este modelo nos ayudará a estandarizar las respuestas de error.

---

## Estructura Final de la Carpeta Models

Después de crear todos los archivos, tu estructura debería verse así:

```
WebApi/
├── Models/
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── UsuarioInfo.cs
│   └── ErrorResponse.cs
├── Controllers/
├── Program.cs
└── appsettings.json
```

---

## Verificación

Después de crear todos los modelos, ejecuta:

```bash
dotnet build
```

**Si compila sin errores**, los modelos están creados correctamente. ✅

---

## ¿Qué hace cada modelo?

- **LoginRequest**: Define qué datos espera el endpoint `/api/login` (usuario y password)
- **UsuarioInfo**: Define la estructura de información del usuario
- **LoginResponse**: Define qué datos retorna el endpoint cuando el login es exitoso
- **ErrorResponse**: Define la estructura de los mensajes de error

---

## Próximo Paso

Una vez que todos los modelos estén creados y compilen correctamente, continuaremos creando el controlador de login.

