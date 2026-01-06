# PASO 5: Verificar y Corregir Configuración de JWT

## Problema Identificado

El error `System.ArgumentNullException` ocurre porque:

1. `appsettings.Development.json` no contiene `SecretKey`
2. Aunque `appsettings.json` tiene `SecretKey`, en desarrollo puede no estar leyéndose correctamente
3. `Program.cs` lanza una excepción si `SecretKey` es null

## Soluciones Aplicadas

### Solución 1: Agregar SecretKey a appsettings.Development.json ✅

He actualizado `appsettings.Development.json` para incluir todas las configuraciones necesarias:

```json
{
  "Jwt": {
    "SecretKey": "MiClaveSecretaSuperSeguraParaJWT12345678901234567890",
    "Issuer": "bradial.mx",
    "Audience": "bradial.mx",
    "ExpirationMinutes": "120"
  }
}
```

### Solución 2: Mejorar Program.cs con validación ✅

He actualizado `Program.cs` para:
- Proporcionar un valor por defecto en desarrollo si no se encuentra `SecretKey`
- Validar que `secretKey` no sea null antes de convertir a bytes
- Mostrar un mensaje de error más claro

## Cómo Funciona la Jerarquía de Configuración

ASP.NET Core carga los archivos de configuración en este orden:

1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (sobrescribe el base)
   - En desarrollo: `appsettings.Development.json`
   - En producción: `appsettings.Production.json`

**Importante**: Si `appsettings.Development.json` define una clave, **sobrescribe** completamente la del `appsettings.json` base.

## Verificación

Después de hacer los cambios, ejecuta:

```bash
dotnet build
dotnet run
```

Si la aplicación inicia sin errores, la configuración está correcta.

## Estructura Recomendada

### appsettings.json (Base)
```json
{
  "Jwt": {
    "SecretKey": "ClaveParaProduccion",
    "Issuer": "miapi.com",
    "Audience": "miapi.com",
    "ExpirationMinutes": "60"
  }
}
```

### appsettings.Development.json (Desarrollo)
```json
{
  "Jwt": {
    "SecretKey": "ClaveParaDesarrollo",
    "Issuer": "bradial.mx",
    "Audience": "bradial.mx",
    "ExpirationMinutes": "120"
  }
}
```

### appsettings.Production.json (Producción)
```json
{
  "Jwt": {
    "SecretKey": "ClaveSuperSecretaDeProduccion",
    "Issuer": "miapi.com",
    "Audience": "miapi.com",
    "ExpirationMinutes": "30"
  }
}
```

## ⚠️ IMPORTANTE

- **Nunca** subas `appsettings.Production.json` con claves reales a repositorios públicos
- Usa **variables de entorno** o **Azure Key Vault** para producción
- Usa claves diferentes para desarrollo y producción

