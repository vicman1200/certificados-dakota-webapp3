import axios from 'axios'

// Determinar la URL base según el entorno
// En desarrollo: localhost
// En producción (quasar build): Azure API
const getBaseURL = () => {
  if (process.env.DEV) {
    // Desarrollo: API local
    return 'http://localhost:5045/api'
  } else if (process.env.PROD) {
    // Producción: API en Azure
    return 'https://bradial-webapi.azurewebsites.net/api'
  }
  // Fallback (por si acaso)
  return 'http://localhost:5045/api'
}

// Crear instancia de axios con configuración base
const api = axios.create({
  baseURL: getBaseURL(),
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
    // PERO no redirigir si el error es del endpoint de login (credenciales incorrectas)
    if (error.response?.status === 401) {
      const isLoginEndpoint = error.config?.url?.includes('/login')
      
      // Si no es el endpoint de login, significa que el token expiró o es inválido
      if (!isLoginEndpoint) {
        localStorage.removeItem('authToken')
        localStorage.removeItem('userInfo')
        
        // Solo redirigir si no estamos ya en la página de login
        // Usar hash para Vue Router en modo hash
        const currentPath = window.location.hash || window.location.pathname
        if (!currentPath.includes('/login')) {
          window.location.href = '/#/login'
        }
      }
      // Si es el endpoint de login, NO redirigir (el error ya se maneja en LoginPage)
    }
    return Promise.reject(error)
  }
)

export default api

