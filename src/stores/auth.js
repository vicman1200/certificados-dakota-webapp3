import { defineStore } from 'pinia'
import { authService } from 'src/services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    isAuthenticated: false,
    loading: false,
    error: null,
    rolId: null,
    agencias: [],
    agenciaSeleccionada: null,
    perfiles: null,
    divisiones: null
  }),

  getters: {
    /**
     * Obtiene el nombre del usuario autenticado
     */
    userName: (state) => {
      return state.user?.nombre || state.user?.usuario || null
    },

    /**
     * Verifica si hay un error de autenticación
     */
    hasError: (state) => {
      return state.error !== null
    }
  },

  actions: {
    /**
     * Inicia sesión con credenciales
     * @param {string} usuario - Nombre de usuario
     * @param {string} password - Contraseña
     */
    async login(usuario, password) {
      this.loading = true
      this.error = null

      try {
        const response = await authService.login(usuario, password)
        
        this.user = response.usuario || { usuario }
        this.isAuthenticated = true
        
        // Guardar rolId si viene en el response (puede estar en response.rolId o response.usuario.rolId)
        const rolId = response.rolId !== undefined 
          ? response.rolId 
          : (response.usuario?.rolId !== undefined ? response.usuario.rolId : null)
        
        if (rolId !== null && rolId !== undefined) {
          this.rolId = rolId
        } else {
          // Intentar obtener del localStorage si no viene en el response
          this.rolId = authService.getRolId()
        }
        
        // Guardar agencias si vienen en el response
        if (response.agencias && Array.isArray(response.agencias)) {
          this.agencias = response.agencias
        } else {
          // Intentar obtener del localStorage si no viene en el response
          this.agencias = authService.getAgencias() || []
        }
        
        // Guardar perfiles si vienen en el response
        if (response.perfiles !== undefined) {
          this.perfiles = response.perfiles
        } else {
          // Intentar obtener del localStorage si no viene en el response
          this.perfiles = authService.getPerfiles()
        }
        
        // Guardar divisiones si vienen en el response
        if (response.divisiones !== undefined) {
          this.divisiones = response.divisiones
        } else {
          // Intentar obtener del localStorage si no viene en el response
          this.divisiones = authService.getDivisiones()
        }
        
        return { success: true, data: response }
      } catch (error) {
        this.error = error.response?.data?.mensaje || 
                    error.response?.data?.message || 
                    'Error al iniciar sesión. Verifica tus credenciales.'
        
        this.isAuthenticated = false
        this.user = null
        
        return { 
          success: false, 
          error: this.error,
          status: error.response?.status 
        }
      } finally {
        this.loading = false
      }
    },

    /**
     * Cierra sesión del usuario
     */
    logout() {
      authService.logout()
      this.user = null
      this.isAuthenticated = false
      this.error = null
      this.rolId = null
      this.agencias = []
      this.agenciaSeleccionada = null
      this.perfiles = null
      this.divisiones = null
    },

    /**
     * Verifica el estado de autenticación al cargar la aplicación
     * Útil para mantener la sesión después de recargar la página
     */
    checkAuth() {
      const token = authService.getToken()
      const userInfo = authService.getUserInfo()
      const rolId = authService.getRolId()
      const agencias = authService.getAgencias()
      const agenciaSeleccionada = authService.getAgenciaSeleccionada()
      const perfiles = authService.getPerfiles()
      const divisiones = authService.getDivisiones()
      
      if (token) {
        this.isAuthenticated = true
        this.user = userInfo
        this.rolId = rolId
        this.agencias = agencias || []
        this.agenciaSeleccionada = agenciaSeleccionada
        this.perfiles = perfiles
        this.divisiones = divisiones
      } else {
        this.isAuthenticated = false
        this.user = null
        this.rolId = null
        this.agencias = []
        this.agenciaSeleccionada = null
        this.perfiles = null
        this.divisiones = null
      }
    },

    /**
     * Selecciona una agencia
     * @param {object} agencia - Objeto de agencia a seleccionar
     */
    seleccionarAgencia(agencia) {
      this.agenciaSeleccionada = agencia
      authService.setAgenciaSeleccionada(agencia)
    },

    /**
     * Limpia el error de autenticación
     */
    clearError() {
      this.error = null
    }
  }
})

