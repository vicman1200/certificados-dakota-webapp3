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
        
        // Guardar agencias si vienen en el response
        if (response.data.agencias && Array.isArray(response.data.agencias)) {
          localStorage.setItem('agencias', JSON.stringify(response.data.agencias))
        }
        
        // Guardar perfiles si vienen en el response
        if (response.data.perfiles) {
          localStorage.setItem('perfiles', JSON.stringify(response.data.perfiles))
        }
        
        // Guardar divisiones si vienen en el response
        if (response.data.divisiones) {
          localStorage.setItem('divisiones', JSON.stringify(response.data.divisiones))
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
    localStorage.removeItem('agencias')
    localStorage.removeItem('agenciaSeleccionada')
    localStorage.removeItem('perfiles')
    localStorage.removeItem('divisiones')
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
  },

  /**
   * Obtiene las agencias guardadas
   * @returns {array|null}
   */
  getAgencias() {
    const agencias = localStorage.getItem('agencias')
    return agencias ? JSON.parse(agencias) : null
  },

  /**
   * Guarda la agencia seleccionada
   * @param {object} agencia - Objeto de agencia seleccionada
   */
  setAgenciaSeleccionada(agencia) {
    if (agencia) {
      localStorage.setItem('agenciaSeleccionada', JSON.stringify(agencia))
    } else {
      localStorage.removeItem('agenciaSeleccionada')
    }
  },

  /**
   * Obtiene la agencia seleccionada
   * @returns {object|null}
   */
  getAgenciaSeleccionada() {
    const agencia = localStorage.getItem('agenciaSeleccionada')
    return agencia ? JSON.parse(agencia) : null
  },

  /**
   * Obtiene los perfiles guardados
   * @returns {any|null}
   */
  getPerfiles() {
    const perfiles = localStorage.getItem('perfiles')
    return perfiles ? JSON.parse(perfiles) : null
  },

  /**
   * Obtiene las divisiones guardadas
   * @returns {any|null}
   */
  getDivisiones() {
    const divisiones = localStorage.getItem('divisiones')
    return divisiones ? JSON.parse(divisiones) : null
  }
}

