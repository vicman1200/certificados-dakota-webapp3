import api from './api'
/**
 * Servicio para gestión de coberturas
 */

export const catalogoCoberturasService = {
    async obtenerCoberturas() {
    try {
      const response = await api.get('/coberturasCat')
      return response.data
    } catch (error) {
      // Propagar el error para que el componente lo maneje
      throw error
    }
  }
 
}


