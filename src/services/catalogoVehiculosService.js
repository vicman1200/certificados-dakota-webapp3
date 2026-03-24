import api from './api'

/**
 * Servicio para catálogo de vehículos
 */
export const catalogoVehiculosService = {
  /**
   * Busca vehículos por modelo (año), marca y opcionalmente subtipo
   * @param {number} modelo - Año del modelo (ej. 2024)
   * @param {string} marca - Marca del vehículo
   * @param {string|null|undefined} subtipo - Subtipo (opcional)
   * @returns {Promise<{code: number, message: string, vehiculos: array}>}
   */
  async buscarVehiculos(modelo, marca, subtipo = null) {
    const params = { modelo, marca }
    if (subtipo != null && String(subtipo).trim() !== '') {
      params.subtipo = String(subtipo).trim()
    }
    const response = await api.get('/catalogo-vehiculos', { params })
    return response.data
  }
}
