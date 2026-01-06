# PASO 9: Conectar el Frontend con la API

## Objetivo
Actualizar la configuración del frontend para que se conecte a tu API local y probar que todo funcione correctamente.

---

## Paso 9.1: Actualizar la URL en api.js

Necesitamos cambiar la URL de la API en el archivo `src/services/api.js`.

### Código actualizado para `src/services/api.js`:

```javascript
import axios from 'axios'

// Crear instancia de axios con configuración base
// IMPORTANTE: Cambia el puerto según donde corra tu API
const api = axios.create({
  baseURL: 'https://localhost:7254/api',  // Cambia 7254 por tu puerto si es diferente
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

**Cambio principal:**
- Línea 5: `baseURL: 'https://localhost:7254/api'` (antes era `https://miapi.com/api`)

---

## Paso 9.2: Verificar el puerto de tu API

Antes de continuar, verifica en qué puerto está corriendo tu API:

1. Ve a la carpeta de tu API:
```bash
cd C:\Users\atack\source\repos\bradial-webapi
```

2. Ejecuta la API:
```bash
dotnet run
```

3. Busca en la consola un mensaje como:
```
Now listening on: https://localhost:7254
```

**Si el puerto es diferente a 7254**, actualiza la línea 5 en `api.js` con el puerto correcto.

---

## Paso 9.3: Verificar CORS en la API

Asegúrate de que tu API permita peticiones desde tu frontend.

En `Program.cs` de tu API, verifica que tengas estos orígenes:

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

## Paso 9.4: Probar la Conexión

### Paso 9.4.1: Iniciar la API

1. Abre una terminal en la carpeta de tu API:
```bash
cd C:\Users\atack\source\repos\bradial-webapi
dotnet run
```

2. Verifica que la API esté corriendo:
   - Deberías ver: `Now listening on: https://localhost:7254`
   - Deja esta terminal abierta (la API debe seguir corriendo)

### Paso 9.4.2: Iniciar el Frontend

1. Abre otra terminal en la carpeta del frontend:
```bash
cd C:\Users\atack\source\repos\bradial-certs-webapp
npm run dev
```

O si usas Quasar CLI:
```bash
quasar dev
```

2. Verifica que el frontend esté corriendo:
   - Deberías ver algo como: `App running at http://localhost:9000`

### Paso 9.4.3: Probar el Login

1. Abre tu navegador y ve a: `http://localhost:9000/login`

2. Ingresa las credenciales:
   - **Usuario**: `admin`
   - **Contraseña**: `admin123`

3. Haz clic en "Iniciar Sesión"

4. **Qué debería pasar:**
   - ✅ El botón muestra "Validando credenciales..." (loading)
   - ✅ Se hace la petición a la API
   - ✅ La API responde con un token JWT
   - ✅ El token se guarda en localStorage
   - ✅ Se muestra una notificación de éxito
   - ✅ Se redirige al dashboard

---

## Paso 9.5: Verificar Errores Comunes

### Error 1: CORS Policy

**Síntoma:**
```
Access to XMLHttpRequest at 'https://localhost:7254/api/login' from origin 'http://localhost:9000' has been blocked by CORS policy
```

**Solución:**
1. Verifica que el puerto del frontend esté en la lista de CORS en `Program.cs`
2. Reinicia la API después de cambiar la configuración de CORS
3. Verifica que uses `AllowCredentials()` en la configuración de CORS

### Error 2: Certificado SSL

**Síntoma:**
```
net::ERR_CERT_AUTHORITY_INVALID
```

**Solución:**
En desarrollo, puedes:
- Hacer clic en "Avanzado" → "Continuar al sitio (no seguro)" en el navegador
- O cambiar a HTTP temporalmente:
  - En `api.js`: `baseURL: 'http://localhost:5045/api'`
  - (Usa el puerto HTTP de tu API, generalmente 5045)

### Error 3: Connection Refused

**Síntoma:**
```
net::ERR_CONNECTION_REFUSED
```

**Solución:**
1. Verifica que la API esté corriendo
2. Verifica que el puerto en `api.js` coincida con el puerto de la API
3. Verifica que no haya un firewall bloqueando la conexión

### Error 4: 401 Unauthorized

**Síntoma:**
La petición falla con código 401

**Solución:**
1. Verifica las credenciales (usuario: `admin`, contraseña: `admin123`)
2. Verifica en la consola del navegador (F12) qué está enviando la petición
3. Verifica en la API que los logs muestren el error

---

## Paso 9.6: Verificar en las DevTools del Navegador

1. Abre las DevTools (F12)
2. Ve a la pestaña **Network** (Red)
3. Intenta hacer login
4. Busca la petición `POST /api/login`
5. Verifica:
   - **Status**: Debe ser `200 OK` si todo funciona
   - **Request Payload**: Debe tener `{"usuario":"admin","password":"admin123"}`
   - **Response**: Debe tener `{"token":"...","usuario":{...}}`

---

## Paso 9.7: Verificar el Token en localStorage

1. En las DevTools, ve a la pestaña **Application** (Aplicación)
2. En el menú lateral, ve a **Local Storage** → `http://localhost:9000`
3. Busca la clave `authToken`
4. Deberías ver un token JWT largo

---

## ✅ Verificación Final

Si todo funciona correctamente:

1. ✅ El login funciona sin errores
2. ✅ El token se guarda en localStorage
3. ✅ Se redirige al dashboard
4. ✅ No hay errores en la consola del navegador
5. ✅ No hay errores en la consola de la API

---

## Próximos Pasos

Una vez que el login funcione correctamente:

1. Podemos crear endpoints protegidos
2. Verificar que el token se use correctamente en peticiones subsecuentes
3. Mejorar la seguridad
4. Agregar más funcionalidades

