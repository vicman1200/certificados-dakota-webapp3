# Documentación del Sistema de Autenticación

## Estructura del Proyecto

El sistema de autenticación está organizado de la siguiente manera para facilitar la reutilización:

```
src/
├── services/
│   ├── api.js              # Configuración de Axios con interceptores
│   └── authService.js      # Servicio de autenticación reutilizable
├── stores/
│   └── auth.js             # Store de Pinia para estado de autenticación
├── pages/
│   ├── LoginPage.vue       # Página de login
│   └── DashboardPage.vue   # Dashboard protegido
└── router/
    ├── routes.js           # Definición de rutas
    └── index.js            # Router con guards de navegación
```

## Almacenamiento del Token JWT

### ¿Por qué localStorage?

Se utiliza `localStorage` para almacenar el token JWT porque:

1. **Persistencia**: El token se mantiene incluso después de cerrar el navegador
2. **Simplicidad**: Fácil de implementar y usar
3. **Compatibilidad**: Funciona en todos los navegadores modernos
4. **Adecuado para aplicaciones web**: Ideal para SPAs (Single Page Applications)

### Alternativas y Consideraciones

#### 1. **sessionStorage** (Alternativa)
```javascript
// En lugar de localStorage
sessionStorage.setItem('authToken', token)
// Se limpia al cerrar la pestaña del navegador
```

**Cuándo usar**: Si quieres que la sesión expire al cerrar el navegador.

#### 2. **Cookies HTTP-Only** (Más seguro)
```javascript
// Requiere configuración en el backend
// El token no es accesible desde JavaScript
// Protege contra XSS attacks
```

**Cuándo usar**: Para aplicaciones con requisitos de seguridad más altos.

#### 3. **Vuex/Pinia** (Solo en memoria)
```javascript
// El token se pierde al recargar la página
```

**Cuándo usar**: Si no necesitas persistencia entre recargas.

### Recomendaciones de Seguridad

⚠️ **Aunque usamos localStorage, debes considerar:**

1. **HTTPS**: Siempre usar HTTPS en producción para proteger el token en tránsito
2. **Token expiration**: Los tokens JWT deben tener un tiempo de expiración corto
3. **Refresh tokens**: Implementar refresh tokens para renovar el JWT sin volver a hacer login
4. **XSS Protection**: Implementar Content Security Policy (CSP) para prevenir XSS attacks

## Uso de los Servicios

### 1. Servicio API (`src/services/api.js`)

Este servicio configura Axios con:
- URL base de la API
- Interceptores para agregar el token automáticamente
- Manejo de errores 401 (token inválido/expirado)

**Uso:**
```javascript
import api from 'src/services/api'

// Todas las peticiones incluyen automáticamente el token
const response = await api.get('/usuarios')
const data = await api.post('/productos', { nombre: 'Producto' })
```

### 2. Servicio de Autenticación (`src/services/authService.js`)

Servicio reutilizable para todas las operaciones de autenticación.

**Métodos disponibles:**
- `login(usuario, password)`: Inicia sesión y guarda el token
- `logout()`: Cierra sesión y limpia el almacenamiento
- `isAuthenticated()`: Verifica si hay un token guardado
- `getToken()`: Obtiene el token actual
- `getUserInfo()`: Obtiene la información del usuario

**Uso:**
```javascript
import { authService } from 'src/services/authService'

// En cualquier componente
const token = authService.getToken()
const isAuth = authService.isAuthenticated()
```

### 3. Store de Autenticación (`src/stores/auth.js`)

Store de Pinia para manejar el estado global de autenticación.

**Estado:**
- `user`: Información del usuario
- `isAuthenticated`: Estado de autenticación
- `loading`: Estado de carga
- `error`: Mensajes de error

**Acciones:**
- `login(usuario, password)`: Inicia sesión
- `logout()`: Cierra sesión
- `checkAuth()`: Verifica el estado de autenticación
- `clearError()`: Limpia los errores

**Uso en componentes:**
```javascript
import { useAuthStore } from 'src/stores/auth'

const authStore = useAuthStore()

// Acceder al estado
const isAuth = authStore.isAuthenticated
const userName = authStore.userName

// Usar acciones
await authStore.login('usuario', 'password')
authStore.logout()
```

## Rutas Protegidas

Las rutas que requieren autenticación están marcadas con `meta: { requiresAuth: true }`:

```javascript
{
  path: 'dashboard',
  component: () => import('pages/DashboardPage.vue'),
  meta: { requiresAuth: true }
}
```

El guard de navegación en `src/router/index.js` protege automáticamente estas rutas.

## Flujo de Autenticación

1. **Usuario ingresa credenciales** → `LoginPage.vue`
2. **Se llama a `authStore.login()`** → Valida con la API
3. **API retorna token JWT** → Se guarda en `localStorage`
4. **Store actualiza estado** → `isAuthenticated = true`
5. **Router redirige** → `/dashboard`
6. **Peticiones subsecuentes** → Incluyen automáticamente el token (interceptor)
7. **Si el token expira (401)** → Se limpia y redirige a `/login`

## Configuración de la API

Para cambiar la URL de la API, edita `src/services/api.js`:

```javascript
const api = axios.create({
  baseURL: 'https://miapi.com/api',  // Cambiar aquí
  // ...
})
```

## Próximos Pasos Recomendados

1. **Implementar refresh tokens** para renovar el JWT sin hacer login
2. **Agregar manejo de expiración** del token antes de que expire
3. **Implementar "Remember me"** con tokens de larga duración
4. **Agregar 2FA** (autenticación de dos factores)
5. **Implementar roles y permisos** basados en el JWT

