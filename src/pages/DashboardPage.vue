<template>
  <q-page class="dashboard-page">
      <q-table
        :rows="rows"
        :columns="columns"
        :filter="filter"
        row-key="uid"
        :rows-per-page-options="[10, 25, 50, 100]"
        class="dashboard-table"
        dense
        :loading="loading"
        flat
        bordered
      >
        <template v-slot:top-left>
          <div class="row q-gutter-md items-center">
            <q-btn
              :style="{ backgroundColor: '#ff8000', color: 'white' }"
              push
              icon="add"
              label="Nuevo certificado"
              dense
              @click="abrirDialog"
            />
            <q-input
              v-model="filtroFechaDesde"
              label="Fecha de expedición Desde"
              type="date"
              outlined
              dense
              stack-label
              class="q-ml-md"
            />
            <q-input
              v-model="filtroFechaHasta"
              label="Fecha de expedición Hasta"
              type="date"
              outlined
              dense
              stack-label
            />
            <q-btn
              color="primary"
              icon="search"
              label="Buscar"
              dense
              :disable="!filtroFechaDesde || !filtroFechaHasta"
              @click="buscarCertificados"
            />
          </div>
        </template>

        <template v-slot:top-right>
          <div class="row q-gutter-md items-center">
            <q-input
              v-model="filter"
              outlined
              dense
              debounce="300"
              placeholder="Buscar..."
              style="min-width: 250px"
            >
              <template v-slot:append>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn
              color="primary"
              icon="file_download"
              dense
              push
              round
              @click="exportarCSV"
            >
              <q-tooltip>Exportar CSV</q-tooltip>
            </q-btn>
          </div>
        </template>

        <template v-slot:body-cell-titular="props">
          <q-td :props="props">
            <a
              href="#"
              class="titular-link"
              @click.prevent="editarCertificado(props.row)"
            >
              {{ props.value }}
            </a>
          </q-td>
        </template>

        <template v-slot:body-cell-fechaExpedicion="props">
          <q-td :props="props">
            {{ formatDate(props.value) }}
          </q-td>
        </template>

        <template v-slot:body-cell-vigenteDesde="props">
          <q-td :props="props">
            {{ formatDate(props.value) }}
          </q-td>
        </template>

        <template v-slot:body-cell-vigenteHasta="props">
          <q-td :props="props">
            {{ formatDate(props.value, 'DD/MM/YYYY') }}
          </q-td>
        </template>

        <template v-slot:body-cell-tipoVehiculo="props">
          <q-td :props="props">
            {{ props.row.tipoVehiculo === 'NU' ? 'NUEVO' : props.row.tipoVehiculo === 'SE' ? 'SEMINUEVO' : props.value || '' }}
          </q-td>
        </template>

        <template v-slot:body-cell-acciones="props">
          <q-td :props="props">
            <q-btn
              flat
              dense
              round
              icon="picture_as_pdf"
              color="red"
              @click="descargarCertificado(props.row)"
            >
              <q-tooltip>Descargar certificado</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>

    <!-- Dialog para nuevo certificado o modificación -->
    <q-dialog v-model="dialogOpen" persistent>
      <q-card style="min-width: 650px; max-width: 650px;">
        <q-card-section>
          <div class="text-h6">{{ modoEdicion ? 'Modificación de certificado' : 'Nuevo certificado' }}</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-form ref="formRef" @submit="guardarCertificado" class="q-gutter-md">
            <!-- Titular y Número de contrato -->
            <div class="row q-gutter-sm" style="display: flex; flex-wrap: nowrap;">
              <div style="flex: 0 0 50%; max-width: 50%; padding-right: 8px;">
                <q-input
                  ref="titularRef"
                  v-model="formulario.titular"
                  label="Titular"
                  outlined
                  dense
                  :rules="[val => !!val || 'El titular es requerido']"
                  lazy-rules
                  hint="Escriba el nombre completo del titular"
                  stack-label
                  autofocus
                />
              </div>
              <div style="flex: 0 0 35%; max-width: 35%;">
                <q-input
                  ref="numeroContratoRef"
                  v-model="formulario.numeroContrato"
                  label="Número de contrato"
                  outlined
                  dense
                  stack-label
                  :rules="[val => !!val || 'El número de contrato es requerido']"
                  lazy-rules
                  hint="Escriba el número de contrato"
                />
              </div>
            </div>

            <!-- Fecha expedición y Años vigencia -->
            <div class="row q-gutter-md">
              <div class="col">
                <q-input
                  ref="fechaExpedicionRef"
                  v-model="formulario.fechaExpedicion"
                  label="Fecha expedición"
                  type="date"
                  outlined
                  dense
                  :rules="[val => !!val || 'La fecha de expedición es requerida']"
                  lazy-rules
                  readonly
                  @update:model-value="calcularFechas"
                />
              </div>
              <div class="col">
                <q-select
                  ref="aniosVigenciaRef"
                  v-model="formulario.aniosVigencia"
                  :options="opcionesAnios"
                  label="Años vigencia"
                  outlined
                  dense
                  :rules="[val => val !== null && val !== undefined && val !== '' || 'Los años de vigencia son requeridos']"
                  lazy-rules
                  emit-value
                  map-options
                  @update:model-value="calcularFechas"
                >
                  <template v-slot:option="scope">
                    <q-item
                      v-bind="scope.itemProps"
                      class="bg-blue-grey-2 text-bold"
                    >
                      <q-item-section>
                        <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
            </div>

            <!-- Vigente desde y Vigente hasta (calculados) -->
            <div class="row q-gutter-md">
              <div class="col">
                <q-input
                  v-model="formulario.vigenteDesde"
                  label="Vigente desde"
                  type="date"
                  outlined
                  dense
                  readonly
                />
              </div>
              <div class="col">
                <q-input
                  v-model="formulario.vigenteHasta"
                  label="Vigente hasta"
                  type="date"
                  outlined
                  dense
                  readonly
                />
              </div>
            </div>

            <!-- Tipo de Vehículo, Marca y Submarca -->
            <div class="row q-gutter-md">
              <div class="col">
                <q-select
                  ref="tipoVehiculoRef"
                  v-model="formulario.tipoVehiculo"
                  :options="opcionesTipoVehiculo"
                  label="Tipo de Vehículo"
                  outlined
                  dense
                  :rules="[val => !!val || 'El tipo de vehículo es requerido']"
                  lazy-rules
                  emit-value
                  map-options
                >
                  <template v-slot:option="scope">
                    <q-item
                      v-bind="scope.itemProps"
                      class="bg-blue-grey-2 text-bold"
                    >
                      <q-item-section>
                        <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
              <div class="col">
                <q-select
                  ref="marcaRef"
                  v-model="formulario.marca"
                  :options="opcionesMarcas"
                  label="Marca"
                  outlined
                  dense
                  :rules="[val => !!val || 'La marca es requerida']"
                  lazy-rules
                  emit-value
                  map-options
                >
                  <template v-slot:option="scope">
                    <q-item
                      v-bind="scope.itemProps"
                      class="bg-blue-grey-2 text-bold"
                    >
                      <q-item-section>
                        <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
              <div class="col">
                <q-select
                  ref="submarcaRef"
                  v-model="formulario.submarca"
                  :options="opcionesSubmarcasFiltradas"
                  label="Submarca"
                  outlined
                  dense
                  :rules="[val => !!val || 'La submarca es requerida']"
                  lazy-rules
                  use-input
                  input-debounce="0"
                  @filter="filtrarSubmarcas"
                  @new-value="crearNuevaSubmarca"
                  new-value-mode="add-unique"
                  fill-input
                  hide-selected
                  emit-value
                  map-options
                  :disable="!formulario.marca"
                >
                  <template v-slot:option="scope">
                    <q-item
                      v-bind="scope.itemProps"
                      class="bg-blue-grey-2 text-bold"
                    >
                      <q-item-section>
                        <q-item-label class="text-brown-9">{{ scope.opt.label }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                  <template v-slot:no-option>
                    <q-item>
                      <q-item-section class="text-grey">
                        Escriba para buscar o presione Enter para agregar
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
            </div>

            <!-- Modelo y No. de serie -->
            <div class="row q-gutter-md">
              <div class="col">
                <q-input
                  ref="modeloRef"
                  v-model="formulario.modelo"
                  label="Modelo"
                  outlined
                  dense
                  :rules="[val => !!val || 'El modelo es requerido']"
                  lazy-rules
                />
              </div>
              <div class="col">
                <q-input
                  ref="numeroSerieRef"
                  v-model="formulario.numeroSerie"
                  label="No. de serie"
                  outlined
                  dense
                  :rules="[val => !!val || 'El número de serie es requerido']"
                  lazy-rules
                />
              </div>
            </div>

            <!-- Botones de acción -->
            <q-card-actions align="right" class="q-pt-md">
              <q-btn
                flat
                label="Cancelar"
                color="negative"
                dense
                push
                @click="cerrarDialog"
              />
              <q-btn
                type="submit"
                :label="modoEdicion ? 'Actualizar' : 'Guardar'"
                :style="{ backgroundColor: '#ff8000', color: 'white' }"
                dense
                push
                :loading="guardando"
              />
            </q-card-actions>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from 'src/stores/auth'
import { authService } from 'src/services/authService'
import { certificadoService } from 'src/services/certificadoService'
import { useQuasar, date } from 'quasar'

const router = useRouter()
const authStore = useAuthStore()
const $q = useQuasar()

const loading = ref(false)
const filter = ref('')
const dialogOpen = ref(false)
const guardando = ref(false)
const modoEdicion = ref(false)
const certificadoEditando = ref(null) // Guardar el uid del certificado que se está editando

// Filtros de fecha para búsqueda
const filtroFechaDesde = ref('')
const filtroFechaHasta = ref('')

// Refs para los campos del formulario
const formRef = ref(null)
const titularRef = ref(null)
const numeroContratoRef = ref(null)
const fechaExpedicionRef = ref(null)
const aniosVigenciaRef = ref(null)
const tipoVehiculoRef = ref(null)
const marcaRef = ref(null)
const submarcaRef = ref(null)
const modeloRef = ref(null)
const numeroSerieRef = ref(null)

// Formulario para nuevo certificado
const formulario = ref({
  titular: '',
  numeroContrato: '',
  fechaExpedicion: '',
  aniosVigencia: null,
  vigenteDesde: '',
  vigenteHasta: '',
  tipoVehiculo: '',
  marca: '',
  submarca: '',
  modelo: '',
  numeroSerie: '',
  usuario: '',
  creadoPor: '',
  estado: 'Solicitado'
})

// Opciones para años de vigencia
const opcionesAnios = [ 
  { label: '2', value: 2 }
]

// Opciones para tipo de vehículo
const opcionesTipoVehiculo = [
  { label: 'Nuevo', value: 'NU' },
  { label: 'Seminuevo', value: 'SE' }
]

// Opciones para marca
const opcionesMarcas = [
  { label: 'Honda', value: 'Honda' },
  { label: 'Acura', value: 'Acura' }
]

// Estructura de submarcas por marca
const submarcasPorMarca = {
  honda: [
    { modelo: 'City', segmento: 'Sedán compacto', categoria: 'Automóvil', descripcion: 'Sedán práctico y eficiente, ideal para ciudad.' },
    { modelo: 'Civic', segmento: 'Sedán mediano', categoria: 'Automóvil', descripcion: 'Sedán con enfoque en tecnología, seguridad y desempeño.' },
    { modelo: 'Civic Hybrid', segmento: 'Sedán mediano híbrido', categoria: 'Automóvil', descripcion: 'Versión híbrida del Civic con mejor eficiencia.' },
    { modelo: 'Accord', segmento: 'Sedán grande', categoria: 'Automóvil', descripcion: 'Sedán premium con alto nivel de confort.' },
    { modelo: 'Accord Hybrid', segmento: 'Sedán grande híbrido', categoria: 'Automóvil', descripcion: 'Versión híbrida del Accord orientada a eficiencia.' },
    { modelo: 'BR-V', segmento: 'SUV compacta de 3 filas', categoria: 'SUV', descripcion: 'SUV familiar con espacio para 7 pasajeros.' },
    { modelo: 'HR-V', segmento: 'SUV compacta', categoria: 'SUV', descripcion: 'SUV juvenil, enfocada en ciudad y eficiencia.' },
    { modelo: 'CR-V', segmento: 'SUV mediana', categoria: 'SUV', descripcion: 'SUV muy popular por su balance entre espacio y equipamiento.' },
    { modelo: 'CR-V Hybrid', segmento: 'SUV mediana híbrida', categoria: 'SUV', descripcion: 'Versión híbrida de la CR-V con mejor rendimiento.' },
    { modelo: 'Odyssey', segmento: 'Minivan', categoria: 'Minivan', descripcion: 'Minivan familiar con espacio y comodidad.' },
    { modelo: 'Pilot', segmento: 'SUV grande', categoria: 'SUV', descripcion: 'SUV de 3 filas, ideal para familias y viajes largos.' }
  ],
  acura: [
    { modelo: 'INTEGRA', segmento: 'Liftback deportivo premium', categoria: 'Automóvil', descripcion: 'Liftback deportivo con enfoque en manejo y diseño.' },
    { modelo: 'TLX', segmento: 'Sedán deportivo premium', categoria: 'Automóvil', descripcion: 'Sedán de lujo orientado a desempeño.' },
    { modelo: 'ADX', segmento: 'SUV compacta premium', categoria: 'SUV', descripcion: 'SUV premium moderna enfocada a un público joven.' },
    { modelo: 'RDX', segmento: 'SUV mediana premium', categoria: 'SUV', descripcion: 'Crossover deportiva con diseño y tecnología de lujo.' },
    { modelo: 'MDX', segmento: 'SUV grande premium de 3 filas', categoria: 'SUV', descripcion: 'SUV insignia de Acura con alto nivel de lujo.' }
  ]
}

// Opciones de submarcas filtradas (se actualiza según la marca seleccionada y el filtro de búsqueda)
const opcionesSubmarcasFiltradas = ref([])

// Computed para obtener las submarcas según la marca seleccionada (ordenadas alfabéticamente)
const opcionesSubmarcas = computed(() => {
  if (!formulario.value.marca) {
    return []
  }
  
  const marcaKey = formulario.value.marca.toLowerCase()
  const submarcas = submarcasPorMarca[marcaKey] || []
  
  // Ordenar alfabéticamente por modelo
  const submarcasOrdenadas = [...submarcas].sort((a, b) => {
    return a.modelo.localeCompare(b.modelo, 'es', { sensitivity: 'base' })
  })
  
  // Convertir a formato para q-select: { label, value }
  return submarcasOrdenadas.map(item => ({
    label: item.modelo,
    value: item.modelo
  }))
})

// Función para filtrar submarcas mientras el usuario escribe
const filtrarSubmarcas = (val, update) => {
  if (val === '') {
    update(() => {
      opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value.filter(
      v => v.label.toLowerCase().indexOf(needle) > -1
    )
  })
}

// Función para crear una nueva submarca cuando el usuario escribe algo que no está en la lista
const crearNuevaSubmarca = (val, done) => {
  if (val && val.length > 0) {
    // Agregar el nuevo valor como opción temporal para que se pueda seleccionar
    const nuevaOpcion = { label: val, value: val }
    // Verificar que no exista ya en las opciones
    const existe = opcionesSubmarcasFiltradas.value.some(opt => opt.value === val)
    if (!existe) {
      opcionesSubmarcasFiltradas.value.push(nuevaOpcion)
    }
    // Aceptar el valor y asignarlo directamente al formulario
    done(val, 'add-unique')
  }
}

// Watcher para actualizar las opciones filtradas cuando cambie la marca
watch(() => formulario.value.marca, (nuevaMarca, marcaAnterior) => {
  // Solo limpiar la submarca si la marca realmente cambió (no es la primera asignación)
  if (marcaAnterior !== undefined && marcaAnterior !== nuevaMarca) {
    formulario.value.submarca = ''
  }
  // Actualizar las opciones filtradas con todas las submarcas de la nueva marca
  if (nuevaMarca) {
    opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value
  } else {
    opcionesSubmarcasFiltradas.value = []
  }
})

const rows = ref([])

// Definición de columnas (basadas en el response del API)
const columns = [
  {
    name: 'uid',
    required: true,
    label: 'ID',
    align: 'left',
    field: row => row.uid,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'noCertificado',
    required: true,
    label: 'Número de Certificado',
    align: 'left',
    field: row => row.noCertificado,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'titular',
    required: true,
    label: 'Titular',
    align: 'left',
    field: row => row.titular,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'numeroContrato',
    label: 'Número de contrato',
    align: 'left',
    field: row => row.numeroContrato,
    format: val => val || '',
    sortable: true
  },
  {
    name: 'fechaExpedicion',
    label: 'Fecha Expedición',
    align: 'left',
    field: row => row.fechaExpedicion,
    sortable: true
  },
  {
    name: 'vigenteDesde',
    label: 'Inicio Vigencia',
    align: 'left',
    field: row => row.vigenteDesde,
    sortable: true
  },
  {
    name: 'vigenteHasta',
    label: 'Fin Vigencia',
    align: 'left',
    field: row => row.vigenteHasta,
    sortable: true
  },
  {
    name: 'tipoVehiculo',
    label: 'Tipo Vehículo',
    align: 'left',
    field: row => row.tipoVehiculo,
    format: val => val === 'NU' ? 'NUEVO' : val === 'SE' ? 'SEMINUEVO' : val || '',
    sortable: true
  },
  {
    name: 'marca',
    label: 'Marca',
    align: 'left',
    field: row => row.marca,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'submarca',
    label: 'Submarca',
    align: 'left',
    field: row => row.submarca,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'modelo',
    label: 'Modelo',
    align: 'left',
    field: row => row.modelo,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'serie',
    label: 'Serie',
    align: 'left',
    field: row => row.serie,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'creadoPor',
    label: 'Creado Por',
    align: 'left',
    field: row => row.creadoPor,
    format: val => `${val}`,
    sortable: true
  },
  {
    name: 'acciones',
    label: 'Acciones',
    align: 'center',
    field: 'acciones',
    sortable: false
  }
]

// Función para obtener el color del chip según polizaStatusId
const getPolizaStatusColor = (polizaStatusId) => {
  if (polizaStatusId === 7) {
    return {
      color: 'deep-orange-5',
      textColor: 'white'
    }
  } else if (polizaStatusId === 4) {
    return {
      color: 'red',
      textColor: 'white'
    }
  } else if (polizaStatusId === 3) {
    return {
      color: 'amber-11',
      textColor: 'amber-10'
    }
  } else if (polizaStatusId === 2) {
    return {
      color: 'green',
      textColor: 'white'
    }
  }
  // Color por defecto si no coincide con ninguno
  return {
    color: 'grey',
    textColor: 'white'
  }
}

// Función para exportar los datos a CSV
const exportarCSV = () => {
  if (rows.value.length === 0) {
    $q.notify({
      type: 'warning',
      message: 'No hay datos para exportar',
      position: 'top',
      timeout: 3000
    })
    return
  }

  try {
    // Obtener los nombres de las columnas (excluyendo 'acciones')
    const columnasExportar = columns.filter(col => col.name !== 'acciones')
    const headers = columnasExportar.map(col => col.label).join(',')

    // Convertir cada row a una línea CSV
    const csvRows = rows.value.map(row => {
      return columnasExportar.map(col => {
        let value = col.field ? col.field(row) : row[col.name]
        
        // Formatear fechas si es necesario
        if (col.name === 'fechaExpedicion' || col.name === 'vigenteDesde' || col.name === 'vigenteHasta') {
          value = formatDate(value)
        }
        
        // Si el valor contiene comas, comillas o saltos de línea, envolverlo en comillas
        if (value != null && (String(value).includes(',') || String(value).includes('"') || String(value).includes('\n'))) {
          value = `"${String(value).replace(/"/g, '""')}"`
        }
        
        return value || ''
      }).join(',')
    })

    // Combinar headers y rows
    const csvContent = [headers, ...csvRows].join('\n')

    // Crear blob y descargar
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    
    // Nombre del archivo con fecha actual
    const fechaActual = date.formatDate(new Date(), 'YYYY-MM-DD_HH-mm-ss')
    link.download = `certificados_${fechaActual}.csv`
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    $q.notify({
      type: 'positive',
      message: `Se exportaron ${rows.value.length} certificados a CSV`,
      position: 'top',
      timeout: 3000
    })
  } catch (error) {
    console.error('Error al exportar CSV:', error)
    $q.notify({
      type: 'negative',
      message: 'Error al exportar el archivo CSV',
      position: 'top',
      timeout: 5000
    })
  }
}

// Función para formatear fechas usando Quasar date
const formatDate = (dateString, format = 'DD/MM/YYYY') => {
  if (!dateString) return ''
  // Si viene en formato ISO (con T), Quasar date lo maneja automáticamente
  return date.formatDate(dateString, format)
}

// Función para calcular fechas de vigencia
const calcularFechas = () => {
  if (!formulario.value.fechaExpedicion || !formulario.value.aniosVigencia) {
    formulario.value.vigenteDesde = ''
    formulario.value.vigenteHasta = ''
    return
  }

  // Vigente desde es igual a la fecha de expedición
  formulario.value.vigenteDesde = formulario.value.fechaExpedicion

  // Vigente hasta es fecha de expedición + años de vigencia
  const fechaExpedicion = new Date(formulario.value.fechaExpedicion)
  const fechaHasta = new Date(fechaExpedicion)
  fechaHasta.setFullYear(fechaHasta.getFullYear() + formulario.value.aniosVigencia)
  
  // Formatear a YYYY-MM-DD para el input type="date"
  formulario.value.vigenteHasta = fechaHasta.toISOString().split('T')[0]
}

// Función para abrir el diálogo (modo nuevo)
const abrirDialog = () => {
  modoEdicion.value = false
  certificadoEditando.value = null
  resetearFormulario(true) // Prellenar con fecha de hoy
  dialogOpen.value = true
}

// Función para editar un certificado (abrir diálogo en modo edición)
const editarCertificado = async (row) => {
  modoEdicion.value = true
  certificadoEditando.value = row.uid
  
  // Guardar la submarca antes de asignar la marca (para evitar que el watcher la limpie)
  const submarcaOriginal = row.submarca || ''
  
  // Poblar el formulario con los datos del certificado
  formulario.value = {
    titular: row.titular || '',
    numeroContrato: row.numeroContrato || '',
    fechaExpedicion: row.fechaExpedicion ? formatDateForInput(row.fechaExpedicion) : '',
    aniosVigencia: calcularAniosVigencia(row.vigenteDesde, row.vigenteHasta),
    vigenteDesde: row.vigenteDesde ? formatDateForInput(row.vigenteDesde) : '',
    vigenteHasta: row.vigenteHasta ? formatDateForInput(row.vigenteHasta) : '',
    tipoVehiculo: row.tipoVehiculo || '',
    marca: row.marca || '',
    submarca: '', // Inicializar vacío, se asignará después de cargar las opciones
    modelo: row.modelo || '',
    numeroSerie: row.serie || row.numeroSerie || '',
    usuario: row.usuario || '',
    creadoPor: row.creadoPor || '',
    estado: row.estado || 'Solicitado'
  }
  
  // Usar nextTick para asegurar que el watcher de marca se ejecute primero
  await nextTick()
  
  // Inicializar las opciones filtradas de submarca si hay una marca seleccionada
  if (formulario.value.marca) {
    opcionesSubmarcasFiltradas.value = opcionesSubmarcas.value
    
    // Si la submarca no está en las opciones (texto libre), agregarla
    if (submarcaOriginal && !opcionesSubmarcasFiltradas.value.some(opt => opt.value === submarcaOriginal)) {
      opcionesSubmarcasFiltradas.value.push({
        label: submarcaOriginal,
        value: submarcaOriginal
      })
    }
    
    // Asignar la submarca después de cargar las opciones
    formulario.value.submarca = submarcaOriginal
  } else {
    opcionesSubmarcasFiltradas.value = []
  }
  
  dialogOpen.value = true
}

// Función para formatear fecha para input type="date" (YYYY-MM-DD)
const formatDateForInput = (dateString) => {
  if (!dateString) return ''
  // Si viene en formato ISO (con T), extraer solo la fecha
  if (dateString.includes('T')) {
    return dateString.split('T')[0]
  }
  // Si viene en formato DD/MM/YYYY, convertir a YYYY-MM-DD
  if (dateString.includes('/')) {
    const parts = dateString.split('/')
    if (parts.length === 3) {
      return `${parts[2]}-${parts[1]}-${parts[0]}`
    }
  }
  // Si ya está en formato YYYY-MM-DD, retornar tal cual
  return dateString
}

// Función para calcular años de vigencia basado en las fechas
const calcularAniosVigencia = (vigenteDesde, vigenteHasta) => {
  if (!vigenteDesde || !vigenteHasta) return null
  
  const desde = new Date(vigenteDesde)
  const hasta = new Date(vigenteHasta)
  
  const diffTime = Math.abs(hasta - desde)
  const diffYears = Math.ceil(diffTime / (1000 * 60 * 60 * 24 * 365))
  
  // Redondear al año más cercano (1, 2, 3 o 4)
  if (diffYears <= 1) return 1
  if (diffYears <= 2) return 2
  if (diffYears <= 3) return 3
  return 4
}

// Función para cerrar el diálogo
const cerrarDialog = () => {
  dialogOpen.value = false
  modoEdicion.value = false
  certificadoEditando.value = null
  resetearFormulario()
}

// Función para resetear el formulario
const resetearFormulario = (prellenarFechaHoy = false) => {
  const userInfo = authStore.user || authService.getUserInfo() || {}
  const usuarioNombre = userInfo.usuario || userInfo.nombre || ''
  
  formulario.value = {
    titular: '',
    numeroContrato: '',
    fechaExpedicion: prellenarFechaHoy ? obtenerFechaHoy() : '',
    aniosVigencia: null,
    vigenteDesde: '',
    vigenteHasta: '',
    tipoVehiculo: '',
    marca: '',
    submarca: '',
    modelo: '',
    numeroSerie: '',
    usuario: usuarioNombre,
    creadoPor: usuarioNombre,
    estado: 'Solicitado'
  }
}

// Función para validar todos los campos
const validarCampos = () => {
  const camposFaltantes = []
  
  // Validar cada campo usando las refs
  if (!formulario.value.titular || formulario.value.titular.trim() === '') {
    camposFaltantes.push('Titular')
  }
  if (!formulario.value.numeroContrato || formulario.value.numeroContrato.trim() === '') {
    camposFaltantes.push('Número de contrato')
  }
  if (!formulario.value.fechaExpedicion || formulario.value.fechaExpedicion.trim() === '') {
    camposFaltantes.push('Fecha expedición')
  }
  if (formulario.value.aniosVigencia === null || formulario.value.aniosVigencia === undefined || formulario.value.aniosVigencia === '') {
    camposFaltantes.push('Años vigencia')
  }
  if (!formulario.value.tipoVehiculo || formulario.value.tipoVehiculo.trim() === '') {
    camposFaltantes.push('Tipo de vehículo')
  }
  if (!formulario.value.marca || formulario.value.marca.trim() === '') {
    camposFaltantes.push('Marca')
  }
  if (!formulario.value.submarca || formulario.value.submarca.trim() === '') {
    camposFaltantes.push('Submarca')
  }
  if (!formulario.value.modelo || formulario.value.modelo.trim() === '') {
    camposFaltantes.push('Modelo')
  }
  if (!formulario.value.numeroSerie || formulario.value.numeroSerie.trim() === '') {
    camposFaltantes.push('No. de serie')
  }
  
  return camposFaltantes
}

// Función para guardar o actualizar el certificado
const guardarCertificado = async () => {
  // Validar el formulario usando la ref del form
  const valid = await formRef.value?.validate()
  
  if (!valid) {
    // Obtener campos faltantes
    const camposFaltantes = validarCampos()
    
    if (camposFaltantes.length > 0) {
      const mensaje = camposFaltantes.length === 1 
        ? `Falta capturar el campo: ${camposFaltantes.join(', ')}`
        : `Faltan capturar los siguientes campos: ${camposFaltantes.join(', ')}`
      
      $q.notify({
        type: 'negative',
        message: mensaje,
        position: 'top',
        timeout: 5000
      })
    }
    return
  }
  
  // Mostrar diálogo de confirmación según el modo
  const mensajeConfirmacion = modoEdicion.value
    ? '¿Está seguro que desea actualizar este certificado?'
    : '¿Está seguro que desea dar de alta un nuevo certificado?'
  
  $q.dialog({
    title: 'Confirmar',
    message: mensajeConfirmacion,
    cancel: {
      label: 'Cancelar',
      flat: true
    },
    ok: {
      label: 'Aceptar',
      push: true
    },
    persistent: true
  }).onOk(async () => {
    // Usuario confirmó, proceder con el guardado o actualización
    if (modoEdicion.value) {
      await actualizarCertificado()
    } else {
      await enviarCertificado()
    }
  })
}

// Función para enviar el certificado al API
const enviarCertificado = async () => {
  guardando.value = true
  
  try {
    // Obtener información del usuario autenticado
    const userInfo = authStore.user || authService.getUserInfo() || {}
    const creadoPorEmail = userInfo.email || userInfo.usuario || 'tester.api@dakotamobility.com.mx'
    
    // Formatear fechas al formato ISO requerido (YYYY-MM-DDTHH:mm:ss)
    const formatearFechaISO = (fecha) => {
      if (!fecha) return null
      // Si ya tiene formato ISO, asegurarse de que tenga la hora
      if (fecha.includes('T')) {
        return fecha
      }
      // Si es solo fecha, agregar hora 00:00:00
      return `${fecha}T00:00:00`
    }
    
    // Crear el payload según la especificación
    const payload = {
      creadoPor: creadoPorEmail,
      titular: formulario.value.titular,
      numeroContrato: formulario.value.numeroContrato,
      fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
      vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
      vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
      tipoVehiculo: formulario.value.tipoVehiculo,
      marca: formulario.value.marca,
      submarca: formulario.value.submarca,
      modelo: formulario.value.modelo,
      serie: formulario.value.numeroSerie
    }
    
    // Enviar al API
    const response = await certificadoService.crearCertificado(payload)
    
    // Verificar si fue exitoso (code === 0)
    if (response.code === 0 && response.certificado) {
      // Agregar el certificado al array de rows con Status y polizaStatusId
      const nuevoCertificado = {
        ...response.certificado,
        numeroContrato: formulario.value.numeroContrato, // Asegurar que se incluya
        tipoVehiculo: formulario.value.tipoVehiculo, // Asegurar que se incluya
        polizaStatus: 'Solicitado',
        polizaStatusId: 7
      }
      rows.value.unshift(nuevoCertificado)
      
      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: response.message || 'Certificado creado correctamente',
        position: 'top',
        timeout: 3000
      })
      
      // Cerrar el diálogo y resetear el formulario
      cerrarDialog()
    } else {
      // Error en la respuesta
      $q.notify({
        type: 'negative',
        message: response.message || 'Error al crear el certificado',
        position: 'top',
        timeout: 5000
      })
    }
  } catch (error) {
    console.error('Error al guardar certificado:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al guardar el certificado. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
  } finally {
    guardando.value = false
  }
}

// Función para actualizar el certificado
const actualizarCertificado = async () => {
  guardando.value = true
  
  try {
    // Obtener información del usuario autenticado
    const userInfo = authStore.user || authService.getUserInfo() || {}
    const modificadoPorEmail = userInfo.email || userInfo.usuario || 'tester.api@dakotamobility.com.mx'
    
    // Formatear fechas al formato ISO requerido (YYYY-MM-DDTHH:mm:ss)
    const formatearFechaISO = (fecha) => {
      if (!fecha) return null
      // Si ya tiene formato ISO, asegurarse de que tenga la hora
      if (fecha.includes('T')) {
        return fecha
      }
      // Si es solo fecha, agregar hora 00:00:00
      return `${fecha}T00:00:00`
    }
    
    // Obtener el noCertificado del certificado que se está editando
    const certificadoActual = rows.value.find(r => r.uid === certificadoEditando.value)
    const noCertificado = certificadoActual?.noCertificado || ''
    
    // Crear el payload para actualización
    const payload = {
      uid: certificadoEditando.value,
      noCertificado: noCertificado,
      modificadoPor: modificadoPorEmail,
      titular: formulario.value.titular,
      numeroContrato: formulario.value.numeroContrato,
      fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
      vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
      vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
      tipoVehiculo: formulario.value.tipoVehiculo,
      marca: formulario.value.marca,
      submarca: formulario.value.submarca,
      modelo: formulario.value.modelo,
      serie: formulario.value.numeroSerie
    }
    
    // Enviar al API para actualizar
    const response = await certificadoService.actualizarCertificado(payload)
    
    // Verificar si fue exitoso (message === "success")
    if (response.message === 'success') {
      // Actualizar el certificado en el array de rows con los datos modificados
      const index = rows.value.findIndex(r => r.uid === certificadoEditando.value)
      if (index !== -1) {
        // Actualizar el row con los datos del formulario (ya formateados)
        rows.value[index] = {
          ...rows.value[index], // Mantener datos existentes
          titular: formulario.value.titular,
          numeroContrato: formulario.value.numeroContrato, // Incluir número de contrato
          fechaExpedicion: formatearFechaISO(formulario.value.fechaExpedicion),
          vigenteDesde: formatearFechaISO(formulario.value.vigenteDesde),
          vigenteHasta: formatearFechaISO(formulario.value.vigenteHasta),
          tipoVehiculo: formulario.value.tipoVehiculo, // Incluir tipo de vehículo
          marca: formulario.value.marca,
          submarca: formulario.value.submarca,
          modelo: formulario.value.modelo,
          serie: formulario.value.numeroSerie,
          // Si la respuesta incluye un certificado actualizado, usar esos datos
          ...(response.certificado || {})
        }
      }
      
      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: 'Certificado actualizado correctamente',
        position: 'top',
        timeout: 3000
      })
      
      // Cerrar el diálogo y resetear el formulario
      cerrarDialog()
    } else {
      // Error en la respuesta
      $q.notify({
        type: 'negative',
        message: response.message || 'Error al actualizar el certificado',
        position: 'top',
        timeout: 5000
      })
    }
  } catch (error) {
    console.error('Error al actualizar certificado:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al actualizar el certificado. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
  } finally {
    guardando.value = false
  }
}

// Función para descargar certificado
const descargarCertificado = async (row) => {
  // Mostrar diálogo de confirmación
  $q.dialog({
    title: 'Generar Certificado PDF',
    message: `¿Está seguro que desea generar el certificado PDF para ${row.noCertificado}?`,
    cancel: true,
    persistent: true,
    ok: {
      label: 'Generar',
      color: 'primary',
      push: true
    },
    cancel: {
      label: 'Cancelar',
      flat: true
    }
  }).onOk(async () => {
    // Usuario confirmó, proceder con la generación
    try {
      // Mostrar notificación de carga
      $q.notify({
        type: 'info',
        message: `Generando certificado ${row.noCertificado}...`,
        position: 'top',
        timeout: 2000
      })

      // Llamar al servicio para generar el PDF
      const pdfBlob = await certificadoService.generarPdf(row.noCertificado)

      // Construir el nombre del archivo: {NoCertificado}_{Titular}.pdf
      const titularLimpio = (row.titular || '').replace(/[^a-zA-Z0-9]/g, '_').trim()
      const filename = `${row.noCertificado}_${titularLimpio}.pdf`

      // Crear URL del blob
      const url = window.URL.createObjectURL(pdfBlob)
      
      // Abrir el PDF en una nueva pestaña
      // Nota: Los blob URLs siempre muestran un GUID en la barra de direcciones
      // pero el PDF se abrirá correctamente y el navegador puede mostrar
      // el nombre del archivo en el título de la pestaña
      const newWindow = window.open(url, '_blank')
      
      // Si el navegador bloquea la ventana emergente, descargar el archivo
      if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
        // Fallback: descargar el archivo con el nombre correcto
        const link = document.createElement('a')
        link.href = url
        link.download = filename
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        
        $q.notify({
          type: 'info',
          message: 'El PDF se descargará automáticamente. Por favor, permita ventanas emergentes para abrir en nueva pestaña.',
          position: 'top',
          timeout: 4000
        })
      }
      
      // Limpiar la URL después de un tiempo (la ventana ya la tiene cargada)
      setTimeout(() => {
        window.URL.revokeObjectURL(url)
      }, 2000)

      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: `Certificado ${row.noCertificado} generado correctamente`,
        position: 'top',
        timeout: 3000
      })
    } catch (error) {
      console.error('Error al generar certificado:', error)
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.mensaje || 
                          'Error al generar el certificado. Por favor, intente nuevamente.'
      
      $q.notify({
        type: 'negative',
        message: errorMessage,
        position: 'top',
        timeout: 5000
      })
    }
  })
}

// Función para cargar certificados desde la API
const cargarCertificados = async () => {
  loading.value = true
  
  try {
    // Preparar el payload con las fechas
    const payload = {
      fechaExpedicionDesde: filtroFechaDesde.value || '',
      fechaExpedicionHasta: filtroFechaHasta.value || ''
    }
    
    // Llamar al servicio
    const response = await certificadoService.obtenerCertificados(payload)
    
    // Verificar si fue exitoso (code === 0)
    if (response.code === 0 && response.certificados) {
      rows.value = response.certificados
    } else if (response.code === 0 && Array.isArray(response.data)) {
      // Si la respuesta viene en data en lugar de certificados
      rows.value = response.data
    } else {
      // Si no hay certificados, inicializar array vacío
      rows.value = []
      
      if (response.message) {
        $q.notify({
          type: 'info',
          message: response.message,
          position: 'top',
          timeout: 3000
        })
      }
    }
  } catch (error) {
    console.error('Error al cargar certificados:', error)
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.mensaje || 
                        'Error al cargar los certificados. Por favor, intente nuevamente.'
    
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top',
      timeout: 5000
    })
    
    // En caso de error, inicializar array vacío
    rows.value = []
  } finally {
    loading.value = false
  }
}

// Función para buscar certificados (se ejecuta al hacer clic en el botón Buscar)
const buscarCertificados = () => {
  // Validar que ambas fechas estén capturadas
  if (!filtroFechaDesde.value || !filtroFechaHasta.value) {
    $q.notify({
      type: 'negative',
      message: 'Debe capturar ambas fechas (Desde y Hasta) para realizar la búsqueda',
      position: 'top',
      timeout: 5000
    })
    return
  }
  
  // Validar que la fecha "Desde" no sea mayor que la fecha "Hasta"
  if (new Date(filtroFechaDesde.value) > new Date(filtroFechaHasta.value)) {
    $q.notify({
      type: 'negative',
      message: 'La fecha "Desde" no puede ser mayor que la fecha "Hasta"',
      position: 'top',
      timeout: 5000
    })
    return
  }
  
  cargarCertificados()
}

// Función para obtener el primer día del mes actual en formato YYYY-MM-DD
const obtenerPrimerDiaDelMes = () => {
  const hoy = new Date()
  const primerDia = new Date(hoy.getFullYear(), hoy.getMonth(), 1)
  return primerDia.toISOString().split('T')[0]
}

// Función para obtener la fecha de hoy en formato YYYY-MM-DD
const obtenerFechaHoy = () => {
  const hoy = new Date()
  return hoy.toISOString().split('T')[0]
}

// Verificar autenticación al cargar y asignar fechas por defecto
onMounted(async () => {
  authStore.checkAuth()
  if (!authStore.isAuthenticated) {
    router.push('/login').catch(() => {
      window.location.href = '/#/login'
    })
    return
  }
  
  // Asignar fechas por defecto: primer día del mes actual y fecha de hoy
  filtroFechaDesde.value = obtenerPrimerDiaDelMes()
  filtroFechaHasta.value = obtenerFechaHoy()
  
  // Cargar certificados automáticamente con las fechas por defecto
  await cargarCertificados()
})
</script>

<style scoped>
.dashboard-page {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 64px); /* Altura total menos el header */
  padding: 16px;
}

.dashboard-table {
  width: 100%;
  height: 100%;
  flex: 1;
  min-height: 0;
}

/* Asegurar que la tabla interna sea responsive y ocupe todo el espacio */
.dashboard-table :deep(.q-table__container) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.dashboard-table :deep(.q-table__top) {
  flex-shrink: 0;
}

.dashboard-table :deep(.q-table__middle) {
  flex: 1;
  overflow: auto;
  min-height: 0;
}

.dashboard-table :deep(.q-table__bottom) {
  flex-shrink: 0;
}

/* Estilos para el hipervínculo del titular */
.titular-link {
  color: orangered;
  text-decoration: none;
  cursor: pointer;
  font-weight: 500;
}

.titular-link:hover {
  text-decoration: underline;
  color: #ff4500;
}

.titular-link:active {
  color: #cc3700;
}

/* Estilos para el chip de Status */
.status-chip {
  font-size: 11px;
  letter-spacing: 0.06em;
}

/* Responsive: ajustar en pantallas pequeñas */
@media (max-width: 600px) {
  .dashboard-page {
    padding: 8px;
  }
}
</style>
