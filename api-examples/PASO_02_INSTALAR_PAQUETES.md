# PASO 2: Instalar Paquetes NuGet Necesarios

## Objetivo
Instalar los paquetes NuGet requeridos para implementar autenticación con JWT.

## Paquetes a Instalar

Necesitamos instalar 3 paquetes principales:

1. **Microsoft.AspNetCore.Authentication.JwtBearer** - Para autenticación JWT
2. **System.IdentityModel.Tokens.Jwt** - Para generar y validar tokens JWT
3. **Swashbuckle.AspNetCore** - Para documentación Swagger (ya viene incluido, pero verificaremos)

## Instrucciones

### Opción A: Instalar desde la Terminal

Ejecuta estos comandos uno por uno en la terminal, dentro de la carpeta del proyecto:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
```

```bash
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
```

```bash
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

### Opción B: Instalar desde Visual Studio

1. Haz clic derecho en el proyecto en el **Solution Explorer**
2. Selecciona **"Manage NuGet Packages..."**
3. Ve a la pestaña **"Browse"**
4. Busca e instala cada paquete:
   - `Microsoft.AspNetCore.Authentication.JwtBearer` (versión 8.0.0)
   - `System.IdentityModel.Tokens.Jwt` (versión 7.0.3)
   - `Swashbuckle.AspNetCore` (versión 6.5.0)

### Opción C: Editar el archivo .csproj directamente

1. Abre el archivo `WebApi.csproj`
2. Agrega estas líneas dentro de `<ItemGroup>`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
</ItemGroup>
```

3. Luego ejecuta:
```bash
dotnet restore
```

## Verificación

Para verificar que los paquetes se instalaron correctamente:

```bash
dotnet list package
```

Deberías ver los 3 paquetes listados.

También puedes compilar el proyecto para asegurarte de que todo está bien:

```bash
dotnet build
```

## ¿Qué hace cada paquete?

- **Microsoft.AspNetCore.Authentication.JwtBearer**: Middleware que permite usar JWT para autenticación en ASP.NET Core
- **System.IdentityModel.Tokens.Jwt**: Librería para crear, leer y validar tokens JWT
- **Swashbuckle.AspNetCore**: Genera documentación automática de la API (Swagger UI)

## Próximo Paso

Una vez instalados los paquetes, continuaremos configurando JWT en el archivo `Program.cs`.

