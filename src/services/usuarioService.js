import api from './api'

/**
 * Servicio para gestión de usuarios
 */
export const usuarioService = {
  /**
   * Lista todos los usuarios
   * @returns {Promise<object>} Response con estructura { code, message, usuarios }
   */
  async listarUsuarios() {
    try {
      const response = await api.get('/usuario/lista-usuarios')
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Crea un nuevo usuario
   * @param {object} usuario - Datos del usuario
   * @param {string} usuario.Usuario - Nombre de usuario
   * @param {string} usuario.Password - Contraseña en texto plano
   * @param {string} usuario.Nombre - Nombre completo
   * @param {string} usuario.Email - Correo electrónico
   * @param {number} usuario.RolID - ID del rol (1 = Usuario Supervisor, 2 = Usuario Regular)
   * @returns {Promise<object>}
   */
  async crearUsuario(usuario) {
    try {
      const response = await api.post('/usuario/crea-usuario', {
        Usuario: usuario.Usuario,
        Password: usuario.Password,
        Nombre: usuario.Nombre,
        Email: usuario.Email,
        RolID: usuario.RolID
      })
      
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Actualiza un usuario existente
   * @param {object} usuario - Datos del usuario a actualizar
   * @param {string} usuario.Usuario - Nombre de usuario
   * @param {string} usuario.Password - Contraseña en texto plano (opcional, solo si se quiere cambiar)
   * @param {string} usuario.Nombre - Nombre completo
   * @param {string} usuario.Email - Correo electrónico
   * @param {number} usuario.RolID - ID del rol (1 = Usuario Supervisor, 2 = Usuario Regular)
   * @param {boolean} usuario.Activo - Estado activo/inactivo del usuario
   * @param {number} usuario.UsuarioId - ID del usuario
   * @returns {Promise<object>}
   */
  async actualizarUsuario(usuario) {
    try {
      const payload = {
        Usuario: usuario.Usuario,
        Nombre: usuario.Nombre,
        Email: usuario.Email,
        RolID: usuario.RolID,
        Activo: usuario.Activo,
        UsuarioId: usuario.UsuarioId
      }
      
      // Solo incluir Password si se proporcionó
      if (usuario.Password && usuario.Password.trim() !== '') {
        payload.Password = usuario.Password
      }
      
      const response = await api.post('/usuario/modifica-usuario', payload)
      
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  }
}

