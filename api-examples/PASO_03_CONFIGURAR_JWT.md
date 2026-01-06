# PASO 3: Configurar JWT en Program.cs

## Objetivo
Configurar la autenticación JWT en el archivo `Program.cs` para que la API pueda generar y validar tokens.

## ⚠️ IMPORTANTE: Antes de empezar

Tu `Program.cs` actual probablemente se ve así (lo que viene por defecto):

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Vamos a modificarlo paso a paso.

---

## Paso 3.1: Agregar los using necesarios

**Al inicio del archivo**, agrega estas líneas después de las que ya existen (si las hay):

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
```

**Tu archivo debería empezar así:**
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
```

---

## Paso 3.2: Configurar CORS

**Justo después de** `builder.Services.AddControllers();`, agrega:

```csharp
// Configurar CORS para permitir peticiones desde el frontend Vue.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:9000",      // Puerto por defecto de Quasar
                "http://localhost:8080",      // Puerto alternativo de Vue
                "https://localhost:9000",     // HTTPS Quasar
                "https://localhost:8080"      // HTTPS Vue
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

---

## Paso 3.3: Configurar JWT - Parte 1 (Obtener configuración)

**Después de la configuración de CORS**, agrega:

```csharp
// Configurar autenticación JWT
var secretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? "MiClaveSecretaSuperSeguraParaJWT12345678901234567890"; // Clave temporal por defecto
    
var issuer = builder.Configuration["Jwt:Issuer"] ?? "miapi.com";
var audience = builder.Configuration["Jwt:Audience"] ?? "miapi.com";
var key = Encoding.UTF8.GetBytes(secretKey);
```

---

## Paso 3.4: Configurar JWT - Parte 2 (Configurar autenticación)

**Después de la parte anterior**, agrega:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

---

## Paso 3.5: Agregar Authorization

**Justo después de la configuración de autenticación**, agrega:

```csharp
builder.Services.AddAuthorization();
```

---

## Paso 3.6: Configurar el Pipeline - Parte 1 (CORS)

**Busca la línea** `app.UseHttpsRedirection();` y **ANTES de esa línea**, agrega:

```csharp
// IMPORTANTE: CORS debe ir antes de Authentication y Authorization
app.UseCors("AllowVueApp");
```

---

## Paso 3.7: Configurar el Pipeline - Parte 2 (Authentication)

**Después de** `app.UseHttpsRedirection();`, agrega estas líneas:

```csharp
// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();
```

**NOTA**: Si ya tienes `app.UseAuthorization();`, solo agrega `app.UseAuthentication();` **ANTES** de `app.UseAuthorization();`.

---

## 📋 Orden Final del Pipeline

El orden en el pipeline debe ser exactamente así:

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANTE: Este orden es CRÍTICO
app.UseCors("AllowVueApp");           // 1. CORS primero
app.UseHttpsRedirection();            // 2. Redirección HTTPS
app.UseAuthentication();              // 3. Autenticación
app.UseAuthorization();               // 4. Autorización
app.MapControllers();                 // 5. Mapear controladores

app.Run();
```

---

## ✅ Verificación

Después de hacer todos los cambios, ejecuta:

```bash
dotnet build
```

**Si compila sin errores**, la configuración está correcta. ✅

**Si hay errores**, verifica:
- ✅ Que todos los `using` estén al inicio del archivo
- ✅ Que no haya llaves faltantes `{}`
- ✅ Que todas las comas y puntos y comas estén correctas

---

## 🔍 Ejemplo Completo del Program.cs

Si necesitas ver cómo debería quedar completo, aquí está la estructura:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options => { ... });

// JWT Config
var secretKey = ...;
var key = Encoding.UTF8.GetBytes(secretKey);
builder.Services.AddAuthentication(...).AddJwtBearer(...);
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## ⚠️ IMPORTANTE

- El orden del middleware es **CRÍTICO**. CORS debe ir antes de Authentication.
- La clave secreta temporal funcionará hasta que configuremos `appsettings.json` en el siguiente paso.
- Si ves errores sobre `SecurityTokenExpiredException`, es normal, lo agregaremos en el siguiente paso.
