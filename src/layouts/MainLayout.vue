<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated style="background-color: #191b20;">
      <q-toolbar style="background-color: #191b20;">
        <!-- Botón hamburguesa comentado
        <q-btn
          flat
          dense
          round
          icon="menu"
          aria-label="Menu"
          @click="toggleLeftDrawer"
        />
        -->

        <q-toolbar-title class="row items-center" style="gap: 8px;">
          <img 
            src="https://bradial.mx/imagenes/logo/logo100.png" 
            alt="Bradial Logo" 
            style="height: 40px; max-width: 150px; cursor: pointer;"
            @click="navegarADashboard"
          />
          <q-btn
            v-if="route.path === '/usuario' || route.path.includes('/usuario')"
            flat
            dense
            icon="arrow_back"
            @click="navegarADashboard"
            style="color: #ff8000;"
          />
          <span 
            style="color: #ff8000 !important; cursor: pointer;"
            @click="navegarADashboard"
          >
            {{ tituloPagina }}
          </span>
        </q-toolbar-title>

        <q-space />

        <!-- Información del usuario y agencia -->
        <div v-if="userInfo" class="user-info q-mr-md column">
          <div class="row items-center q-gutter-sm">
            <div 
              class="text-body2 text-white text-weight-medium cursor-pointer usuario-link"
              @click="abrirDialogUsuarioInfo"
            >
              {{ userInfo.nombre || userInfo.usuario }}
            </div>
          </div>
          <!-- Información de la agencia seleccionada -->
          <div 
            v-if="agenciaSeleccionada" 
            class="text-caption text-primary text-weight-bold row items-center q-gutter-xs q-mt-xs cursor-pointer agencia-link"
            @click="abrirDialogAgencia"
          >
            <q-icon name="business" size="14px" />
            <span>{{ agenciaSeleccionada.agencia || '' }}</span>
            <span v-if="agenciaSeleccionada.entidadFederativa">
              - {{ agenciaSeleccionada.entidadFederativa }}
            </span>
            <span v-if="agenciaSeleccionada.division">
              ({{ agenciaSeleccionada.division }})
            </span>
          </div>
        </div>

        <q-btn
          flat
          dense
          round
          icon="logout"
          @click="handleLogout"
        >
          <q-tooltip>Cerrar sesión</q-tooltip>
        </q-btn>
      </q-toolbar>
    </q-header>

    <!-- Menú lateral izquierdo comentado
    <q-drawer
      v-model="leftDrawerOpen"
      show-if-above
      bordered
    >
      <q-list>
        <q-item-label
          header
        >
          Essential Links
        </q-item-label>

        <EssentialLink
          v-for="link in essentialLinks"
          :key="link.title"
          v-bind="link"
        />
      </q-list>
    </q-drawer>
    -->

    <q-page-container>
      <router-view />
    </q-page-container>

    <!-- Dialog para dar de alta usuarios -->
    <q-dialog v-model="dialogUsuario" persistent>
      <q-card style="min-width: 500px; max-width: 500px;">
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">Dar de alta usuario</div>
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
              label="Confirmar Contraseña"
              :type="showConfirmPassword ? 'text' : 'password'"
              outlined
              dense
              stack-label
              maxlength="15"
              :rules="[
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
          </q-form>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat label="Cancelar" color="grey" v-close-popup />
          <q-btn 
            push 
            label="Guardar" 
            color="primary" 
            @click="guardarUsuario"
            :loading="loadingUsuario"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Dialog para selección de agencia -->
    <q-dialog v-model="dialogAgenciaOpen" persistent>
      <q-card style="min-width: 600px; max-width: 800px;">
        <q-banner dense class="bg-black q-pa-md">
          <div class="row items-center justify-between full-width">
            <div class="text-h6 text-primary">Seleccionar Agencia</div>
            <q-btn
              v-if="mostrarBotonCerrar"
              flat
              dense
              round
              size="sm"
              icon="close"
              color="white"
              text-color="white"
              @click="dialogAgenciaOpen = false"
            />
          </div>
        </q-banner>

        <q-card-section>
          <div class="text-subtitle2 text-grey-7">
            Por favor, selecciona una agencia para continuar
          </div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input
            v-model="filtroAgencia"
            outlined
            dense
            debounce="300"
            placeholder="Buscar agencia..."
            class="q-mb-md"
            autofocus
          >
            <template v-slot:prepend>
              <q-icon name="search" />
            </template>
            <template v-slot:append v-if="filtroAgencia">
              <q-icon
                name="clear"
                class="cursor-pointer"
                @click="filtroAgencia = ''"
              />
            </template>
          </q-input>
        </q-card-section>

        <q-card-section class="q-pt-none" style="max-height: 60vh; overflow-y: auto;">
          <q-list separator>
            <q-item
              v-for="(agencia, index) in agenciasFiltradas"
              :key="index"
              clickable
              v-ripple
              @click="seleccionarAgencia(agencia)"
              class="q-pa-sm q-mb-sm"
              style="border: 1px solid #e0e0e0; border-radius: 8px; margin-bottom: 8px;"
            >
              <q-item-section>
                <q-item-label class="text-weight-medium text-body2">
                  {{ agencia.agencia || 'N/A' }}
                </q-item-label>
                <q-item-label caption class="q-mt-xs text-caption">
                  <div class="column q-gutter-xs">
                    <div v-if="agencia.bbvaAgenciaId" class="row items-center">
                      <q-icon name="tag" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">Agencia ID:</span>
                      <span class="q-ml-xs">{{ agencia.bbvaAgenciaId }}</span>
                    </div>
                    <div v-if="agencia.entidadFederativa" class="row items-center">
                      <q-icon name="location_on" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">Estado:</span>
                      <span class="q-ml-xs">{{ agencia.entidadFederativa }}</span>
                    </div>
                    <div v-if="agencia.division" class="row items-center">
                      <q-icon name="business" size="12px" class="q-mr-xs" />
                      <span class="text-weight-medium">División:</span>
                      <span class="q-ml-xs">{{ agencia.division }}</span>
                    </div>
                  </div>
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-icon name="chevron_right" color="primary" size="20px" />
              </q-item-section>
            </q-item>
          </q-list>
          <div v-if="agenciasFiltradas.length === 0" class="text-center text-grey-6 q-pa-md">
            No se encontraron agencias
          </div>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Dialog para información del usuario -->
    <q-dialog v-model="dialogUsuarioInfo" persistent>
      <q-card style="min-width: 400px; max-width: 450px;">
        <q-banner dense class="bg-black q-pa-md">
          <div class="row items-center justify-between full-width">
            <div class="text-h6 text-primary">Información del Usuario</div>
            <q-btn
              flat
              dense
              round
              size="sm"
              icon="close"
              color="white"
              text-color="white"
              @click="dialogUsuarioInfo = false"
            />
          </div>
        </q-banner>

        <q-card-section dense class="q-pt-sm">
          <div class="column q-gutter-sm">
            <div class="row items-center q-gutter-xs">
              <q-icon name="person" color="primary" size="18px" />
              <div class="column">
                <div class="text-caption text-grey-7">Usuario</div>
                <div class="text-body2 text-weight-medium">
                  {{ userInfo?.usuario || 'N/A' }}
                </div>
              </div>
            </div>

            <q-separator dense />

            <div class="row items-center q-gutter-xs">
              <q-icon name="badge" color="primary" size="18px" />
              <div class="column">
                <div class="text-caption text-grey-7">Nombre</div>
                <div class="text-body2 text-weight-medium">
                  {{ userInfo?.nombre || 'N/A' }}
                </div>
              </div>
            </div>

            <q-separator dense />

            <div class="row items-center q-gutter-xs">
              <q-icon name="admin_panel_settings" color="primary" size="18px" />
              <div class="column">
                <div class="text-caption text-grey-7">Perfil</div>
                <div class="text-body2 text-weight-medium">
                  {{ userInfo?.rol || userInfo?.Rol || 'N/A' }}
                </div>
              </div>
            </div>

            <q-separator v-if="agenciaSeleccionada?.division" dense />

            <div v-if="agenciaSeleccionada?.division" class="row items-center q-gutter-xs">
              <q-icon name="business" color="primary" size="18px" />
              <div class="column">
                <div class="text-caption text-grey-7">División</div>
                <div class="text-body2 text-weight-medium">
                  {{ agenciaSeleccionada.division }}
                </div>
              </div>
            </div>
          </div>
        </q-card-section>

        <q-card-actions dense align="right" class="q-pa-sm">
          <q-btn 
            push 
            dense
            label="Cerrar" 
            color="primary" 
            v-close-popup 
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-layout>
</template>

<script>
import { defineComponent, ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from 'src/stores/auth'
import { authService } from 'src/services/authService'
import { usuarioService } from 'src/services/usuarioService'
import { useQuasar } from 'quasar'
import EssentialLink from 'components/EssentialLink.vue'

const linksList = [
  {
    title: 'Docs',
    caption: 'quasar.dev',
    icon: 'school',
    link: 'https://quasar.dev'
  },
  {
    title: 'Github',
    caption: 'github.com/quasarframework',
    icon: 'code',
    link: 'https://github.com/quasarframework'
  },
  {
    title: 'Discord Chat Channel',
    caption: 'chat.quasar.dev',
    icon: 'chat',
    link: 'https://chat.quasar.dev'
  },
  {
    title: 'Forum',
    caption: 'forum.quasar.dev',
    icon: 'record_voice_over',
    link: 'https://forum.quasar.dev'
  },
  {
    title: 'Twitter',
    caption: '@quasarframework',
    icon: 'rss_feed',
    link: 'https://twitter.quasar.dev'
  },
  {
    title: 'Facebook',
    caption: '@QuasarFramework',
    icon: 'public',
    link: 'https://facebook.quasar.dev'
  },
  {
    title: 'Quasar Awesome',
    caption: 'Community Quasar projects',
    icon: 'favorite',
    link: 'https://awesome.quasar.dev'
  }
]

export default defineComponent({
  name: 'MainLayout',

  components: {
    EssentialLink
  },

  setup () {
    const leftDrawerOpen = ref(false)
    const router = useRouter()
    const route = useRoute()
    const authStore = useAuthStore()
    const $q = useQuasar()
    const dialogUsuario = ref(false)
    const formUsuarioRef = ref(null)
    const loadingUsuario = ref(false)
    const showPassword = ref(false)
    const showConfirmPassword = ref(false)
    const confirmarPassword = ref('')
    const dialogAgenciaOpen = ref(false)
    const filtroAgencia = ref('')
    const mostrarBotonCerrar = ref(false)
    const dialogUsuarioInfo = ref(false)

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
      RolID: null
    })

    // Obtener información del usuario desde localStorage o store
    const userInfo = computed(() => {
      // Primero intentar obtener del store
      if (authStore.user) {
        return authStore.user
      }
      // Si no está en el store, obtener de localStorage
      return authService.getUserInfo()
    })

    // Obtener agencia seleccionada desde el store
    const agenciaSeleccionada = computed(() => {
      return authStore.agenciaSeleccionada || authService.getAgenciaSeleccionada()
    })

    // Obtener agencias del store
    const agencias = computed(() => {
      return authStore.agencias || []
    })

    // Filtrar agencias según el texto de búsqueda
    const agenciasFiltradas = computed(() => {
      if (!filtroAgencia.value || filtroAgencia.value.trim() === '') {
        return agencias.value
      }
      
      const filtro = filtroAgencia.value.toLowerCase().trim()
      
      return agencias.value.filter(agencia => {
        const nombreAgencia = (agencia.agencia || '').toLowerCase()
        const id = (agencia.bbvaAgenciaId || '').toString().toLowerCase()
        const estado = (agencia.entidadFederativa || '').toLowerCase()
        const division = (agencia.division || '').toLowerCase()
        
        return nombreAgencia.includes(filtro) ||
               id.includes(filtro) ||
               estado.includes(filtro) ||
               division.includes(filtro)
      })
    })

    // Función para abrir el diálogo de selección de agencia
    const abrirDialogAgencia = () => {
      filtroAgencia.value = '' // Limpiar filtro al abrir
      mostrarBotonCerrar.value = true // Mostrar botón cuando se abre desde el vínculo
      dialogAgenciaOpen.value = true
    }

    // Función para abrir el diálogo de información del usuario
    const abrirDialogUsuarioInfo = () => {
      dialogUsuarioInfo.value = true
    }

    // Función para seleccionar una agencia
    const seleccionarAgencia = (agencia) => {
      authStore.seleccionarAgencia(agencia)
      dialogAgenciaOpen.value = false
      
      $q.notify({
        type: 'positive',
        message: `Agencia ${agencia.agencia || ''} seleccionada`,
        position: 'top',
        timeout: 2000
      })
    }

    // Verificar si el usuario puede dar de alta usuarios (rolId === 1)
    const puedeDarAltaUsuarios = computed(() => {
      // Primero intentar obtener del store
      if (authStore.rolId !== null && authStore.rolId !== undefined) {
        return authStore.rolId === 1
      }
      // Si no está en el store, obtener de localStorage
      const rolId = authService.getRolId()
      return rolId === 1
    })

    // Título dinámico según la ruta actual
    const tituloPagina = computed(() => {
      if (route.path === '/usuario' || route.path.includes('/usuario')) {
        return 'Gestión de Usuarios'
      }
      return 'Gestión de Certificados'
    })

    // Verificar autenticación al montar el componente
    onMounted(() => {
      authStore.checkAuth()
    })

    const handleLogout = () => {
      authStore.logout()
      $q.notify({
        type: 'info',
        message: 'Sesión cerrada correctamente',
        position: 'top'
      })
      router.push('/login').catch(() => {
        window.location.href = '/#/login'
      })
    }

    const navegarADashboard = () => {
      router.push('/dashboard').catch(() => {
        window.location.href = '/#/dashboard'
      })
    }

    const navegarAUsuarios = () => {
      router.push('/usuario').catch(() => {
        window.location.href = '/#/usuario'
      })
    }

    const abrirDialogUsuario = () => {
      // Limpiar formulario
      nuevoUsuario.value = {
        Usuario: '',
        Password: '',
        Nombre: '',
        Email: '',
        RolID: null
      }
      confirmarPassword.value = ''
      showPassword.value = false
      showConfirmPassword.value = false
      dialogUsuario.value = true
    }

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
          nuevoUsuario.value = {
            Usuario: '',
            Password: '',
            Nombre: '',
            Email: '',
            Rol: '',
            RolID: null
          }
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

    return {
      essentialLinks: linksList,
      leftDrawerOpen,
      userInfo,
      agenciaSeleccionada,
      agencias,
      puedeDarAltaUsuarios,
      dialogUsuario,
      dialogAgenciaOpen,
      formUsuarioRef,
      nuevoUsuario,
      loadingUsuario,
      showPassword,
      showConfirmPassword,
      confirmarPassword,
      opcionesPerfiles,
      route,
      toggleLeftDrawer () {
        leftDrawerOpen.value = !leftDrawerOpen.value
      },
      handleLogout,
      navegarADashboard,
      navegarAUsuarios,
      abrirDialogUsuario,
      guardarUsuario,
      abrirDialogAgencia,
      seleccionarAgencia,
      filtroAgencia,
      agenciasFiltradas,
      mostrarBotonCerrar,
      abrirDialogUsuarioInfo,
      dialogUsuarioInfo,
      tituloPagina
    }
  }
})
</script>

<style scoped>
.agencia-link {
  text-decoration: underline;
  transition: opacity 0.2s;
}

.agencia-link:hover {
  opacity: 0.8;
}

.usuario-link {
  transition: opacity 0.2s;
  text-decoration: underline;
}

.usuario-link:hover {
  opacity: 0.8;
}
</style>
