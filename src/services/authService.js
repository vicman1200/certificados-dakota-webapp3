import api from './api'

/**
 * Servicio de autenticación
 * Reutilizable en toda la aplicación
 */
export const authService = {
  /**
   * Inicia sesión con credenciales de usuario
   * @param {string} usuario - Nombre de usuario
   * @param {string} password - Contraseña
   * @returns {Promise<{token: string, usuario: object}>}
   */
  async login(usuario, password) {
    try {
      const response = await api.post('/login', {
        usuario,
        password
      })
      
      // Guardar token en localStorage
      if (response.data.token) {
        localStorage.setItem('authToken', response.data.token)
        
        // Guardar información del usuario incluyendo rolId
        if (response.data.usuario) {
          localStorage.setItem('userInfo', JSON.stringify(response.data.usuario))
        }
        
        // Guardar rolId si viene en el response (puede estar en response.data.rolId o response.data.usuario.rolId)
        const rolId = response.data.rolId !== undefined 
          ? response.data.rolId 
          : (response.data.usuario?.rolId !== undefined ? response.data.usuario.rolId : null)
        
        if (rolId !== null && rolId !== undefined) {
          localStorage.setItem('rolId', rolId.toString())
        }
      }
      
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Cierra sesión del usuario
   */
  logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('userInfo')
    localStorage.removeItem('rolId')
  },

  /**
   * Verifica si el usuario está autenticado
   * @returns {boolean}
   */
  isAuthenticated() {
    return !!localStorage.getItem('authToken')
  },

  /**
   * Obtiene el token actual
   * @returns {string|null}
   */
  getToken() {
    return localStorage.getItem('authToken')
  },

  /**
   * Obtiene la información del usuario guardada
   * @returns {object|null}
   */
  getUserInfo() {
    const userInfo = localStorage.getItem('userInfo')
    return userInfo ? JSON.parse(userInfo) : null
  },

  /**
   * Obtiene el rolId del usuario actual
   * @returns {number|null}
   */
  getRolId() {
    const rolId = localStorage.getItem('rolId')
    return rolId ? parseInt(rolId, 10) : null
  }
}

