# Paquetes NuGet Requeridos

Este documento lista todos los paquetes NuGet necesarios para la Web API de autenticación.

## 📋 Lista de Paquetes

### 1. Microsoft.AspNetCore.Authentication.JwtBearer (v8.0.0)
**Descripción**: Middleware para autenticación basada en tokens JWT en ASP.NET Core.

**Instalación**:
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
```

**Uso**: Se utiliza en `Program.cs` para configurar la autenticación JWT.

---

### 2. System.IdentityModel.Tokens.Jwt (v7.0.3)
**Descripción**: Biblioteca para crear, validar y leer tokens JWT.

**Instalación**:
```bash
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
```

**Uso**: Se utiliza en `Services/JwtService.cs` para generar y validar tokens.

---

### 3. Swashbuckle.AspNetCore (v6.5.0)
**Descripción**: Genera automáticamente documentación Swagger/OpenAPI para la API.

**Instalación**:
```bash
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

**Uso**: Se utiliza en `Program.cs` para configurar Swagger UI.

---

## 📦 Paquetes Opcionales (Para Base de Datos)

### Microsoft.EntityFrameworkCore (v8.0.0)
**Descripción**: ORM para trabajar con bases de datos.

**Instalación**:
```bash
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
```

**Cuándo usar**: Cuando implementes acceso a base de datos para almacenar usuarios.

---

### Microsoft.EntityFrameworkCore.SqlServer (v8.0.0)
**Descripción**: Proveedor de Entity Framework para SQL Server.

**Instalación**:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
```

**Cuándo usar**: Si usarás SQL Server como base de datos.

---

### Microsoft.EntityFrameworkCore.Tools (v8.0.0)
**Descripción**: Herramientas de línea de comandos para Entity Framework (migrations, etc.).

**Instalación**:
```bash
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
```

**Cuándo usar**: Para generar y aplicar migraciones de base de datos.

---

## 🔧 Instalación Masiva

Para instalar todos los paquetes requeridos de una vez:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

O simplemente restaurar desde el archivo `.csproj`:

```bash
dotnet restore
```

---

## ✅ Verificar Instalación

Para verificar que los paquetes están instalados:

```bash
dotnet list package
```

---

## 🔄 Actualizar Paquetes

Para actualizar todos los paquetes a la última versión:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Swashbuckle.AspNetCore
```

---

## 📝 Notas

- Las versiones especificadas son compatibles con .NET 8.0
- Si usas .NET 7.0, ajusta las versiones de los paquetes
- Los paquetes de Entity Framework son opcionales y solo necesarios si implementarás base de datos

