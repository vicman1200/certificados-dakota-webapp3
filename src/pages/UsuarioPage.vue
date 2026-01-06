<template>
  <q-page class="usuario-page">
    <q-table
      :rows="rows"
      :columns="columns"
      :filter="filter"
      row-key="usuarioId"
      :rows-per-page-options="[10, 25, 50, 100]"
      class="usuario-table"
      dense
      :loading="loading"
      flat
      bordered
    >
      <template v-slot:top-left>
        <q-btn
          :style="{ backgroundColor: '#ff8000', color: 'white' }"
          push
          icon="add"
          label="Agregar usuario"
          dense
          @click="abrirDialogUsuario"
        />
      </template>

      <template v-slot:top-right>
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
      </template>

      <template v-slot:body-cell-nombre="props">
        <q-td :props="props">
          <a
            href="#"
            class="nombre-link text-weight-bold"
            @click.prevent="editarUsuario(props.row)"
          >
            {{ props.value }}
          </a>
        </q-td>
      </template>

      <template v-slot:body-cell-rol="props">
        <q-td :props="props">
          {{ (props.row.rolId === 1 || props.row.RolId === 1) ? 'Usuario Supervisor' : 
              (props.row.rolId === 2 || props.row.RolId === 2) ? 'Usuario Regular' : 
              props.value }}
        </q-td>
      </template>

      <template v-slot:body-cell-activo="props">
        <q-td :props="props">
          <q-chip
            :color="props.row.Activo ? 'green' : 'grey'"
            text-color="white"
            dense
            size="sm"
          >
            {{ props.row.Activo ? 'Activo' : 'Inactivo' }}
          </q-chip>
        </q-td>
      </template>

      <template v-slot:body-cell-fechaCreacion="props">
        <q-td :props="props">
          {{ formatDate(props.row.fechaCreacion) }}
        </q-td>
      </template>
    </q-table>

    <!-- Dialog para dar de alta usuarios -->
    <q-dialog v-model="dialogUsuario" persistent>
      <q-card style="min-width: 500px; max-width: 500px;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">{{ modoEdicion ? 'Modificar usuario' : 'Dar de alta usuario' }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section>
          <q-form ref="formUsuarioRef" @submit.prevent="guardarUsuario" class="q-gutter-md">
            <q-input
              v-model="nuevoUsuario.Usuario"
              label="Usuario"
              outlined
              dense
              stack-label
              maxlength="50"
              :readonly="modoEdicion"
              :rules="[
                val => !!val || 'El usuario es requerido',
                val => (val && val.length >= 4) || 'El usuario debe tener al menos 4 caracteres',
                val => (val && val.length <= 50) || 'El usuario no puede tener más de 50 caracteres'
              ]"
              lazy-rules
              autofocus
            >
              <template v-slot:prepend>
                <q-icon name="person" />
              </template>
            </q-input>

            <q-input
              v-model="nuevoUsuario.Password"
              label="Contraseña"
              :type="showPassword ? 'text' : 'password'"
              outlined
              dense
              stack-label
              maxlength="15"
              :rules="[
                val => !!val || 'La contraseña es requerida',
                val => (val && val.length >= 8) || 'La contraseña debe tener al menos 8 caracteres',
                val => (val && val.length <= 15) || 'La contraseña no puede tener más de 15 caracteres'
              ]"
              lazy-rules
            >
              <template v-slot:prepend>
                <q-icon name="lock" />
              </template>
              <template v-slot:append>
                <q-icon
                  :name="showPassword ? 'visibility' : 'visibility_off'"
                  class="cursor-pointer"
                  @click="showPassword = !showPassword"
                />
              </template>
            </q-input>

            <q-input
              v-model="confirmarPassword"
              :label="modoEdicion ? 'Confirmar Nueva Contraseña' : 'Confirmar Contraseña'"
              :type="showConfirmPassword ? 'text' : 'password'"
              outlined
              dense
              stack-label
              maxlength="15"
              :rules="modoEdicion ? [
                val => !nuevoUsuario.Password || !!val || 'Debe confirmar la contraseña',
                val => !nuevoUsuario.Password || val === nuevoUsuario.Password || 'Las contraseñas no coinciden'
              ] : [
                val => !!val || 'Debe confirmar la contraseña',
                val => val === nuevoUsuario.Password || 'Las contraseñas no coinciden'
              ]"
              lazy-rules
            >
              <template v-slot:prepend>
                <q-icon name="lock" />
              </template>
              <template v-slot:append>
                <q-icon
                  :name="showConfirmPassword ? 'visibility' : 'visibility_off'"
                  class="cursor-pointer"
                  @click="showConfirmPassword = !showConfirmPassword"
                />
              </template>
            </q-input>

            <q-input
              v-model="nuevoUsuario.Nombre"
              label="Nombre"
              outlined
              dense
              stack-label
              maxlength="150"
              :readonly="modoEdicion"
              :rules="[
                val => !!val || 'El nombre es requerido',
                val => (val && val.length >= 6) || 'El nombre debe tener al menos 6 caracteres',
                val => (val && val.length <= 150) || 'El nombre no puede tener más de 150 caracteres'
              ]"
              lazy-rules
            >
              <template v-slot:prepend>
                <q-icon name="badge" />
              </template>
            </q-input>

            <q-input
              v-model="nuevoUsuario.Email"
              label="Email"
              type="email"
              outlined
              dense
              stack-label
              maxlength="150"
              :readonly="modoEdicion"
              :rules="[
                val => !!val || 'El email es requerido',
                val => (val && val.length >= 4) || 'El email debe tener al menos 4 caracteres',
                val => (val && val.length <= 150) || 'El email no puede tener más de 150 caracteres',
                val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || 'El email no es válido'
              ]"
              lazy-rules
            >
              <template v-slot:prepend>
                <q-icon name="email" />
              </template>
            </q-input>

            <q-select
              v-model="nuevoUsuario.RolID"
              :options="opcionesPerfiles"
              label="Perfil"
              outlined
              dense
              stack-label
              emit-value
              map-options
              :rules="[val => val !== null && val !== undefined && val !== '' || 'El perfil es requerido']"
              lazy-rules
            >
              <template v-slot:prepend>
                <q-icon name="admin_panel_settings" />
              </template>
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

            <q-toggle
              v-if="modoEdicion"
              v-model="nuevoUsuario.Activo"
              label="Usuario Activo"
              color="green"
            />
          </q-form>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat label="Cancelar" color="grey" v-close-popup />
          <q-btn 
            push 
            :label="modoEdicion ? 'Actualizar' : 'Guardar'" 
            color="primary" 
            @click="modoEdicion ? actualizarUsuario() : guardarUsuario()"
            :loading="loadingUsuario"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useQuasar } from 'quasar'
import { usuarioService } from 'src/services/usuarioService'
import { date } from 'quasar'

const $q = useQuasar()

// Estado reactivo
const rows = ref([])
const loading = ref(false)
const filter = ref('')
const dialogUsuario = ref(false)
const formUsuarioRef = ref(null)
const loadingUsuario = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const confirmarPassword = ref('')
const modoEdicion = ref(false)
const usuarioEditando = ref(null)

// Opciones de perfiles
const opcionesPerfiles = [
  { label: 'Usuario Regular', value: 2 },
  { label: 'Usuario Supervisor', value: 1 }
]

// Formulario para nuevo usuario
const nuevoUsuario = ref({
  Usuario: '',
  Password: '',
  Nombre: '',
  Email: '',
  RolID: null,
  Activo: true,
  UsuarioId: null
})

// Columnas de la tabla
const columns = [
  {
    name: 'usuario',
    required: true,
    label: 'Usuario',
    align: 'left',
    field: 'usuario',
    sortable: true
  },
  {
    name: 'nombre',
    label: 'Nombre',
    align: 'left',
    field: 'nombre',
    sortable: true
  },
  {
    name: 'email',
    label: 'Email',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'rol',
    label: 'Rol',
    align: 'left',
    field: 'rol',
    sortable: true
  },
  {
    name: 'activo',
    label: 'Estado',
    align: 'center',
    field: 'Activo',
    sortable: true
  },
  {
    name: 'fechaCreacion',
    label: 'Fecha Creación',
    align: 'left',
    field: 'fechaCreacion',
    sortable: true,
    format: val => formatDate(val)
  }
]

// Función para formatear fechas
const formatDate = (dateValue) => {
  if (!dateValue) return '-'
  try {
    return date.formatDate(dateValue, 'DD/MM/YYYY HH:mm')
  } catch (error) {
    return dateValue
  }
}

// Función para cargar usuarios
const cargarUsuarios = async () => {
  loading.value = true
  try {
    const response = await usuarioService.listarUsuarios()
    
    if (response.code === 0 && response.usuarios) {
      rows.value = response.usuarios
    } else {
      $q.notify({
        type: 'negative',
        message: response.message || 'Error al cargar usuarios',
        position: 'top'
      })
    }
  } catch (error) {
    console.error('Error al cargar usuarios:', error)
    const errorMessage = error.response?.data?.mensaje || 
                       error.response?.data?.message || 
                       'Error al cargar la lista de usuarios'
    $q.notify({
      type: 'negative',
      message: errorMessage,
      position: 'top'
    })
  } finally {
    loading.value = false
  }
}

// Función para abrir dialog de nuevo usuario
const abrirDialogUsuario = () => {
  modoEdicion.value = false
  usuarioEditando.value = null
  // Limpiar formulario
  nuevoUsuario.value = {
    Usuario: '',
    Password: '',
    Nombre: '',
    Email: '',
    RolID: null,
    Activo: true,
    UsuarioId: null
  }
  confirmarPassword.value = ''
  showPassword.value = false
  showConfirmPassword.value = false
  dialogUsuario.value = true
}

// Función para editar usuario
const editarUsuario = (usuario) => {
  modoEdicion.value = true
  usuarioEditando.value = usuario
  
  // Poblar formulario con datos del usuario
  nuevoUsuario.value = {
    Usuario: usuario.usuario || usuario.Usuario || '',
    Password: '',
    Nombre: usuario.nombre || usuario.Nombre || '',
    Email: usuario.email || usuario.Email || '',
    RolID: usuario.rolId || usuario.RolId || null,
    Activo: usuario.Activo !== undefined ? usuario.Activo : (usuario.activo !== undefined ? usuario.activo : true),
    UsuarioId: usuario.UsuarioId !== undefined && usuario.UsuarioId !== null ? usuario.UsuarioId : (usuario.usuarioId !== undefined && usuario.usuarioId !== null ? usuario.usuarioId : null)
  }
  
  confirmarPassword.value = ''
  showPassword.value = false
  showConfirmPassword.value = false
  dialogUsuario.value = true
}

// Función para guardar usuario
const guardarUsuario = async () => {
  // Validar formulario
  const valid = await formUsuarioRef.value.validate()
  if (!valid) {
    $q.notify({
      type: 'negative',
      message: 'Por favor, complete todos los campos requeridos',
      position: 'top'
    })
    return
  }

  // Confirmar antes de guardar
  $q.dialog({
    title: 'Confirmar',
    message: `¿Estás seguro que quieres dar de alta al Usuario: ${nuevoUsuario.value.Usuario}?`,
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
    loadingUsuario.value = true
    try {
      await usuarioService.crearUsuario(nuevoUsuario.value)
      
      $q.notify({
        type: 'positive',
        message: 'Usuario creado correctamente',
        position: 'top'
      })
      
      // Cerrar dialog y limpiar formulario
      dialogUsuario.value = false
      modoEdicion.value = false
      usuarioEditando.value = null
      nuevoUsuario.value = {
        Usuario: '',
        Password: '',
        Nombre: '',
        Email: '',
        RolID: null,
        Activo: true,
        UsuarioId: null
      }
      confirmarPassword.value = ''
      
      // Recargar lista de usuarios
      await cargarUsuarios()
    } catch (error) {
      const errorMessage = error.response?.data?.mensaje || 
                         error.response?.data?.message || 
                         'Error al crear el usuario'
      $q.notify({
        type: 'negative',
        message: errorMessage,
        position: 'top'
      })
    } finally {
      loadingUsuario.value = false
    }
  })
}

// Función para actualizar usuario
const actualizarUsuario = async () => {
  // Validar formulario
  const valid = await formUsuarioRef.value.validate()
  if (!valid) {
    $q.notify({
      type: 'negative',
      message: 'Por favor, complete todos los campos requeridos',
      position: 'top'
    })
    return
  }

  // Si se cambió la contraseña, validar confirmación
  if (nuevoUsuario.value.Password && nuevoUsuario.value.Password !== confirmarPassword.value) {
    $q.notify({
      type: 'negative',
      message: 'Las contraseñas no coinciden',
      position: 'top'
    })
    return
  }

  // Confirmar antes de actualizar
  $q.dialog({
    title: 'Confirmar',
    message: `¿Estás seguro que quieres actualizar al Usuario: ${nuevoUsuario.value.Usuario}?`,
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
    loadingUsuario.value = true
    try {
      await usuarioService.actualizarUsuario(nuevoUsuario.value)
      
      $q.notify({
        type: 'positive',
        message: 'Usuario actualizado correctamente',
        position: 'top'
      })
      
      // Cerrar dialog y limpiar formulario
      dialogUsuario.value = false
      modoEdicion.value = false
      usuarioEditando.value = null
      nuevoUsuario.value = {
        Usuario: '',
        Password: '',
        Nombre: '',
        Email: '',
        RolID: null,
        Activo: true,
        UsuarioId: null
      }
      confirmarPassword.value = ''
      
      // Recargar lista de usuarios
      await cargarUsuarios()
    } catch (error) {
      const errorMessage = error.response?.data?.mensaje || 
                         error.response?.data?.message || 
                         'Error al actualizar el usuario'
      $q.notify({
        type: 'negative',
        message: errorMessage,
        position: 'top'
      })
    } finally {
      loadingUsuario.value = false
    }
  })
}

// Cargar usuarios al montar el componente
onMounted(() => {
  cargarUsuarios()
})
</script>

<style scoped>
.usuario-page {
  padding: 16px;
}

.usuario-table {
  width: 100%;
  height: calc(100vh - 100px);
}

.nombre-link {
  color: #ff8000;
  text-decoration: none;
  cursor: pointer;
}

.nombre-link:hover {
  text-decoration: underline;
}
</style>

