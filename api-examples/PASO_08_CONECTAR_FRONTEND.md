# PASO 8: Conectar el Frontend con la API

## Objetivo
Actualizar la configuración del frontend para que se conecte a la API real que acabamos de crear.

---

## Paso 8.1: Identificar la URL de tu API

Primero, necesitamos saber en qué puerto está corriendo tu API.

1. Ejecuta tu API:
```bash
dotnet run
```

2. Busca en la consola un mensaje similar a:
```
Now listening on: https://localhost:7254
Now listening on: http://localhost:5000
```

**Anota el puerto HTTPS** (generalmente `7254` o `5001`).

---

## Paso 8.2: Actualizar la URL en api.js

Necesitamos actualizar el archivo `src/services/api.js` para que apunte a tu API local.

### Código actualizado para src/services/api.js:

```javascript
import axios from 'axios'

// Crear instancia de axios con configuración base
// IMPORTANTE: Cambia el puerto según donde corra tu API
const api = axios.create({
  baseURL: 'https://localhost:7254/api',  // Cambia 7254 por tu puerto
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Interceptor de solicitudes: agrega el token JWT si existe
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Interceptor de respuestas: maneja errores comunes
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    // Si el token es inválido o expiró (401), limpiar y redirigir a login
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken')
      // Solo redirigir si no estamos ya en la página de login
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default api
```

**IMPORTANTE**: 
- Cambia `7254` por el puerto que te mostró tu API
- Si tu API usa HTTP en lugar de HTTPS, cambia a `http://localhost:PUERTO/api`

---

## Paso 8.3: Verificar CORS

Tu API debe estar configurada para aceptar peticiones desde tu frontend.

En `Program.cs`, verifica que tengas estos orígenes en CORS:

```csharp
policy.WithOrigins(
    "http://localhost:9000",      // Puerto por defecto de Quasar
    "http://localhost:8080",      // Puerto alternativo de Vue
    "https://localhost:9000",     // HTTPS Quasar
    "https://localhost:8080"      // HTTPS Vue
)
```

Si tu frontend corre en otro puerto, agrégalo a esta lista.

---

## Paso 8.4: Probar la Conexión

1. **Asegúrate de que la API esté corriendo:**
```bash
dotnet run
```

2. **Inicia tu frontend Quasar:**
```bash
npm run dev
# o
quasar dev
```

3. **Abre tu navegador** y ve a la página de login (generalmente `http://localhost:9000/login`)

4. **Prueba con las credenciales:**
   - Usuario: `admin`
   - Contraseña: `admin123`

---

## Paso 8.5: Verificar Errores de CORS

Si ves un error de CORS en la consola del navegador:

```
Access to XMLHttpRequest at 'https://localhost:7254/api/login' from origin 'http://localhost:9000' has been blocked by CORS policy
```

**Solución:**
1. Verifica que el puerto del frontend esté en la lista de CORS en `Program.cs`
2. Reinicia la API después de cambiar la configuración de CORS
3. Verifica que uses `AllowCredentials()` en la configuración de CORS

---

## Paso 8.6: Verificar Errores de Certificado SSL

Si ves un error sobre certificados SSL (solo en desarrollo):

**Solución temporal para desarrollo:**
En el navegador, cuando veas el error de certificado, puedes:
- Hacer clic en "Avanzado" → "Continuar al sitio (no seguro)"
- O usar HTTP en lugar de HTTPS para desarrollo

**O mejor aún**, configura tu API para usar HTTP en desarrollo:
En `Program.cs`, comenta o modifica la línea:
```csharp
// app.UseHttpsRedirection(); // Comentar en desarrollo si hay problemas con SSL
```

Y actualiza `api.js` para usar HTTP:
```javascript
baseURL: 'http://localhost:5000/api',  // Usar HTTP en desarrollo
```

---

## Verificación Final

Si todo funciona correctamente:

1. ✅ El frontend carga la página de login
2. ✅ Al ingresar credenciales, se hace la petición a la API
3. ✅ La API responde con un token JWT
4. ✅ El token se guarda en localStorage
5. ✅ Se redirige al dashboard

---

## Próximos Pasos

Una vez que el login funcione correctamente, podremos:
- Verificar que el token se use correctamente en peticiones subsecuentes
- Crear endpoints protegidos
- Mejorar la seguridad
- Agregar más funcionalidades

