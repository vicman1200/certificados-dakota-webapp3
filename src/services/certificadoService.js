import api from './api'

/**
 * Servicio para gestión de certificados
 */
export const certificadoService = {
  /**
   * Obtiene certificados filtrados por fecha
   * @param {object} filtros - Filtros de búsqueda { fechaExpedicionDesde, fechaExpedicionHasta }
   * @returns {Promise<{code: number, message: string, certificados: array}>}
   */
  async obtenerCertificados(filtros) {
    try {
      const response = await api.post('/certificados', filtros)
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Crea un nuevo certificado
   * @param {object} certificado - Datos del certificado
   * @returns {Promise<{code: number, message: string, certificado: object}>}
   */
  async crearCertificado(certificado) {
    try {
      const response = await api.post('/crea-certificado', certificado)
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Descarga el PDF de un certificado
   * @param {number} uid - ID del certificado
   * @returns {Promise<Blob>} - Archivo PDF como Blob
   */
  async descargarPdf(uid) {
    try {
      const response = await api.get(`/certificado/${uid}/pdf`, {
        responseType: 'blob' // Importante: indicar que la respuesta es un archivo binario
      })
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Genera el PDF de un certificado
   * @param {string} noCertificado - Número de certificado
   * @returns {Promise<Blob>} - Archivo PDF como Blob
   */
  async generarPdf(noCertificado) {
    try {
      const response = await api.post('/generar', {
        noCertificado: noCertificado
      }, {
        responseType: 'blob' // Importante: indicar que la respuesta es un archivo binario
      })
      
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Actualiza un certificado existente
   * @param {object} certificado - Datos del certificado a actualizar
   * @returns {Promise<{code: number, message: string, certificado: object}>}
   */
  async actualizarCertificado(certificado) {
    try {
      const response = await api.post('/modificar-certificado', certificado)
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Verifica si un contrato existe
   * @param {string} noContrato - Número de contrato a verificar
   * @returns {Promise<{contratoExiste: boolean}>}
   */
  async verificarContrato(noContrato) {
    try {
      const response = await api.get(`/verifica-contrato/${noContrato}`)
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  },

  /**
   * Consulta certificados con filtros de perfiles y divisiones
   * @param {object} filtros - Filtros de búsqueda { perfiles: string[], divisiones: string[] }
   * @returns {Promise<any>}
   */
  async consultarCertificados(filtros) {
    try {
      const response = await api.post('/certificados/consulta', filtros)
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  }
}


