# PASO 1: Crear el Proyecto de Web API

## Objetivo
Crear un nuevo proyecto de Web API con .NET 8

## Instrucciones

### Opción A: Usando la terminal/CMD

1. Abre una terminal (CMD, PowerShell, o Terminal) en la ubicación donde quieres crear el proyecto.

2. Ejecuta el siguiente comando para crear un nuevo proyecto de Web API:

```bash
dotnet new webapi -n WebApi -o .
```

**Nota**: Si ejecutas este comando dentro de la carpeta `api-examples`, creará los archivos base del proyecto allí.

### Opción B: Usando Visual Studio

1. Abre Visual Studio 2022
2. Selecciona **"Create a new project"**
3. Busca y selecciona **"ASP.NET Core Web API"**
4. Haz clic en **Next**
5. Configura:
   - **Project name**: `WebApi`
   - **Location**: La carpeta donde quieres el proyecto
   - **Framework**: `.NET 8.0`
6. Haz clic en **Create**

### Opción C: Usando Visual Studio Code

1. Abre VS Code
2. Abre la terminal integrada (Ctrl + `)
3. Navega a la carpeta donde quieres el proyecto
4. Ejecuta:
```bash
dotnet new webapi -n WebApi
```

## Verificación

Después de crear el proyecto, deberías tener una estructura similar a esta:

```
WebApi/
├── Controllers/
│   └── WeatherForecastController.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
└── WebApi.csproj
```

## Próximo Paso

Una vez que hayas creado el proyecto, confirma que se completó exitosamente ejecutando:

```bash
dotnet restore
dotnet build
```

Si estos comandos se ejecutan sin errores, el proyecto está listo para continuar con el siguiente paso.

