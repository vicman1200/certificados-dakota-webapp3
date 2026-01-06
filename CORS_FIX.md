# Solución para Error de CORS

## Problema Identificado

El error de CORS puede estar causado por:
1. **Orden del middleware** - CORS debe ir en el lugar correcto
2. **UseHttpsRedirection()** - Puede estar redirigiendo las peticiones HTTP a HTTPS antes de que CORS las procese

## Solución

Actualiza el `Program.cs` de tu API con este código en la sección del pipeline:

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANTE: El orden del middleware es CRÍTICO
// CORS debe ir DESPUÉS de UseRouting pero ANTES de UseAuthentication
app.UseHttpsRedirection();

// CORS debe ir después de UseHttpsRedirection pero antes de UseAuthentication
app.UseCors("AllowVueApp");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

**O mejor aún, para desarrollo con HTTP, deshabilita temporalmente UseHttpsRedirection:**

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Comentar UseHttpsRedirection en desarrollo si usas HTTP
    // app.UseHttpsRedirection();
}

// IMPORTANTE: El orden del middleware es CRÍTICO
app.UseCors("AllowVueApp");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Pasos para Aplicar

1. **Edita** `C:\Users\atack\source\repos\bradial-webapi\Program.cs`
2. **Comenta o mueve** `app.UseHttpsRedirection();` 
3. **Asegúrate** que `app.UseCors("AllowVueApp");` esté antes de `UseAuthentication()`
4. **Reinicia** la API completamente (detén y vuelve a iniciar)
5. **Prueba** nuevamente el login

## Verificación

Después de hacer los cambios:
1. Detén la API (Ctrl+C)
2. Vuelve a ejecutar: `dotnet run`
3. Intenta hacer login nuevamente

