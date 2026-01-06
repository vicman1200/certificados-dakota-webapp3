# Implementación de Login con Base de Datos

## Resumen

Esta guía explica cómo implementar la validación de usuarios y contraseñas contra una base de datos SQL Server usando hash BCrypt para almacenar contraseñas de forma segura.

---

## Paso 1: Crear la Tabla de Usuarios

Ejecuta el script SQL `DDL_Usuarios.sql` en tu base de datos SQL Server para crear la tabla de usuarios.

### Campos principales:
- `Usuario`: Nombre de usuario único (usado para login)
- `PasswordHash`: Contraseña hasheada (BCrypt)
- `Nombre`: Nombre completo
- `Email`: Email único
- `Rol`: Rol del usuario (Admin, Usuario, Supervisor, Operador)
- `Activo`: Si el usuario puede iniciar sesión
- `IntentosFallidos`: Contador de intentos fallidos
- `BloqueadoHasta`: Fecha hasta cuando está bloqueado

---

## Paso 2: Instalar Paquetes NuGet

```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package BCrypt.Net-Next
```

O desde Visual Studio:
1. Click derecho en el proyecto → **Manage NuGet Packages**
2. Instalar:
   - `Microsoft.Data.SqlClient` (última versión)
   - `BCrypt.Net-Next` (última versión)

---

## Paso 3: Configurar Connection String

Agrega la connection string en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BradialDB;User Id=sa;Password=TuPassword;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "TuClaveSecretaSuperSegura12345678901234567890",
    "Issuer": "bradial.mx",
    "Audience": "bradial.mx",
    "ExpirationMinutes": "60"
  }
}
```

**Para desarrollo local:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BradialDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

---

## Paso 4: Reemplazar LoginController

Reemplaza el contenido de `Controllers/LoginController.cs` con el código de `LoginController.BD.cs`.

**Importante:** El nuevo controlador:
- ✅ Valida credenciales contra la base de datos
- ✅ Verifica si el usuario está activo
- ✅ Verifica si el usuario está bloqueado
- ✅ Usa BCrypt para verificar contraseñas
- ✅ Bloquea la cuenta después de 5 intentos fallidos
- ✅ Actualiza el último acceso
- ✅ Genera token JWT con información del usuario

---

## Paso 5: Crear Usuarios de Prueba

### Opción A: Script SQL (Manual)

```sql
-- Hash de "Admin123!" usando BCrypt
-- Genera el hash en C# y reemplázalo aquí
INSERT INTO [dbo].[Usuarios] ([Usuario], [PasswordHash], [Nombre], [Email], [Rol], [Activo])
VALUES 
    ('admin', '$2a$11$TuHashBCryptAqui', 'Administrador', 'admin@bradial.mx', 'Admin', 1),
    ('tester', '$2a$11$TuHashBCryptAqui', 'Usuario de Prueba', 'tester@bradial.mx', 'Usuario', 1);
```

### Opción B: Crear un Endpoint de Registro (Solo para desarrollo)

Crea un endpoint temporal para generar usuarios:

```csharp
[HttpPost("crear-usuario")]
public IActionResult CrearUsuario([FromBody] CrearUsuarioRequest request)
{
    // SOLO PARA DESARROLLO - ELIMINAR EN PRODUCCIÓN
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
    
    // Insertar en BD...
    
    return Ok(new { message = "Usuario creado" });
}
```

### Opción C: Usar una herramienta de línea de comandos

Crea un script C# simple para generar hashes:

```csharp
using BCrypt.Net;

string password = "Admin123!";
string hash = BCrypt.Net.BCrypt.HashPassword(password);
Console.WriteLine($"Hash: {hash}");
```

---

## Paso 6: Probar el Login

1. Ejecuta la API
2. Prueba el endpoint `POST /api/login` con:
```json
{
  "usuario": "admin",
  "password": "Admin123!"
}
```

3. Deberías recibir un token JWT y la información del usuario

---

## Seguridad Implementada

✅ **Contraseñas hasheadas**: Usa BCrypt (algoritmo seguro y lento)
✅ **Bloqueo de cuenta**: Después de 5 intentos fallidos, bloquea por 30 minutos
✅ **Validación de estado**: Verifica que el usuario esté activo
✅ **Logging**: Registra intentos de login fallidos
✅ **JWT seguro**: Token con información del usuario y rol

---

## Funcionalidades Adicionales

### Bloqueo de Cuenta
- Después de 5 intentos fallidos, la cuenta se bloquea por 30 minutos
- El bloqueo se resetea automáticamente después del tiempo configurado
- Se resetea inmediatamente después de un login exitoso

### Auditoría
- `FechaUltimoAcceso`: Se actualiza en cada login exitoso
- `IntentosFallidos`: Se incrementa en cada login fallido
- `FechaCreacion`: Fecha de creación del usuario

---

## Próximos Pasos (Opcional)

1. **Restablecimiento de contraseña**: Implementar usando `TokenResetPassword`
2. **Cambio de contraseña**: Endpoint para que usuarios cambien su contraseña
3. **Gestión de usuarios**: CRUD completo de usuarios
4. **Roles y permisos**: Sistema de autorización basado en roles
5. **2FA**: Autenticación de dos factores

---

## Solución de Problemas

### Error: "Connection string 'DefaultConnection' no está configurada"
- Verifica que `appsettings.json` tenga la sección `ConnectionStrings`
- Asegúrate de que la connection string sea correcta

### Error: "BCrypt.Net.BCrypt" no se encuentra
- Instala el paquete NuGet: `BCrypt.Net-Next`
- Verifica que el `using` esté correcto

### Error: "Usuario o contraseña incorrectos" pero las credenciales son correctas
- Verifica que el hash de la contraseña en BD sea correcto
- Asegúrate de usar BCrypt para generar el hash
- Verifica que no haya espacios en blanco en el usuario o contraseña

---

## Archivos Creados

- ✅ `DDL_Usuarios.sql` - Script SQL para crear la tabla
- ✅ `Controllers/LoginController.BD.cs` - Controlador con validación contra BD
- ✅ `IMPLEMENTAR_LOGIN_BD.md` - Esta guía

---

¡Listo! Ya tienes un sistema de autenticación seguro contra base de datos. 🔐

