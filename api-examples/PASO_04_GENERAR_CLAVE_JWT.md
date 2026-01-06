# PASO 4: Generar Clave Secreta para JWT

## Objetivo
Generar una clave secreta segura para firmar los tokens JWT en `appsettings.json`.

## Requisitos de la Clave Secreta

- **Mínimo 32 caracteres** (recomendado: 64 caracteres o más)
- Debe ser **aleatoria** y **secreta**
- **Nunca** compartirla públicamente
- Usar diferentes claves para desarrollo y producción

---

## Métodos para Generar una Clave Secreta

### Método 1: Usando PowerShell (Windows) ✅ Recomendado

1. Abre **PowerShell** (no CMD)
2. Ejecuta este comando:

```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

Esto generará una cadena de 64 caracteres alfanuméricos aleatorios.

**Ejemplo de salida:**
```
Kj8mN2pQ9rT4vW7xY1zA5bC6dE3fG0hI8jK2lM5nO7pQ9rS1tU4vW6xY3zA
```

---

### Método 2: Usando CMD (Windows)

1. Abre **CMD** (Símbolo del sistema)
2. Ejecuta:

```cmd
powershell -Command "-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})"
```

---

### Método 3: Usando .NET CLI (Recomendado para desarrolladores)

Ejecuta este comando en la terminal:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -base64 32)"
```

O si no tienes OpenSSL, usa este comando de PowerShell:

```powershell
dotnet user-secrets set "Jwt:SecretKey" "$(-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_}))"
```

---

### Método 4: Usando Generadores Online (Usar con precaución)

⚠️ **ADVERTENCIA**: Solo usa generadores online si confías en ellos y no compartirás la clave.

Algunos sitios recomendados:
- https://www.guidgenerator.com/online-guid-generator.aspx
- https://www.random.org/strings/
- https://passwordsgenerator.net/

**Configuración recomendada:**
- Longitud: 64 caracteres
- Incluir: Letras mayúsculas, minúsculas, números
- NO incluir símbolos (no son necesarios para JWT)

---

### Método 5: Usando OpenSSL (Si está instalado)

```bash
openssl rand -base64 64
```

O si quieres solo caracteres alfanuméricos:

```bash
openssl rand -hex 32
```

---

### Método 6: Generador en C# (Programa simple)

Crea un archivo temporal `GenerateKey.cs`:

```csharp
using System;
using System.Security.Cryptography;

var key = new byte[64];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
}
var base64Key = Convert.ToBase64String(key);
Console.WriteLine(base64Key);
```

Ejecuta:
```bash
dotnet run --project GenerateKey.cs
```

---

## ✅ Configurar la Clave en appsettings.json

Una vez que tengas tu clave generada, edita el archivo `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "SecretKey": "TU_CLAVE_GENERADA_AQUI_MINIMO_32_CARACTERES",
    "Issuer": "miapi.com",
    "Audience": "miapi.com",
    "ExpirationMinutes": "60"
  }
}
```

**Reemplaza** `TU_CLAVE_GENERADA_AQUI_MINIMO_32_CARACTERES` con la clave que generaste.

---

## 🔒 Seguridad - Recomendaciones

### Para Desarrollo

Puedes usar una clave simple en `appsettings.json` o `appsettings.Development.json`:

```json
{
  "Jwt": {
    "SecretKey": "MiClaveSecretaParaDesarrollo12345678901234567890",
    "ExpirationMinutes": "120"
  }
}
```

### Para Producción

⚠️ **NUNCA** hardcodees la clave en `appsettings.Production.json` si el código está en un repositorio público.

**Opciones seguras:**

1. **Variables de Entorno:**
   ```bash
   # Windows
   set Jwt__SecretKey=TuClaveSuperSecretaAqui
   
   # Linux/Mac
   export Jwt__SecretKey=TuClaveSuperSecretaAqui
   ```

2. **User Secrets (Solo para desarrollo local):**
   ```bash
   dotnet user-secrets set "Jwt:SecretKey" "TuClaveSecreta"
   ```

3. **Azure Key Vault** (Para Azure)
4. **AWS Secrets Manager** (Para AWS)
5. **Variables de entorno en el servidor** (Recomendado)

---

## 📝 Ejemplo Completo de appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "SecretKey": "Kj8mN2pQ9rT4vW7xY1zA5bC6dE3fG0hI8jK2lM5nO7pQ9rS1tU4vW6xY3zA5bC7dE9fG1hI3jK",
    "Issuer": "miapi.com",
    "Audience": "miapi.com",
    "ExpirationMinutes": "60"
  }
}
```

---

## 🧪 Verificar que Funciona

Después de agregar la clave, ejecuta:

```bash
dotnet build
dotnet run
```

Si la aplicación inicia sin errores, la configuración está correcta.

---

## ⚠️ IMPORTANTE

- ✅ **Mínimo 32 caracteres** (preferible 64+)
- ✅ **Diferente para desarrollo y producción**
- ✅ **Nunca compartir públicamente**
- ✅ **No subir a repositorios públicos** (usar .gitignore o variables de entorno)
- ✅ **Rotar periódicamente** en producción

