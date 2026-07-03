<template>
  <div class="login-container">
    <q-card class="login-card q-pa-xl" flat>
      <!-- Logo -->
      <div class="logo-section text-center q-mb-xl">
        <img 
          src="https://dakotamobility.com.mx/imagenes/logo/logoconletra.png" 
          alt="Dakota Mobility Logo" 
          class="logo-image"
        />
        <div class="login-title text-h4 text-weight-bold q-mt-md">
          Iniciar Sesión
        </div>
        <div class="login-subtitle text-subtitle2 q-mt-xs">
          Ingresa tus credenciales para continuar
        </div>
      </div>

      <!-- Formulario de Login -->
      <q-form @submit.prevent="onSubmit" class="login-form q-gutter-md">
        <!-- Campo Usuario -->
        <q-input
          v-model="usuario"
          label="Usuario"
          outlined
          dense
          :rules="[val => !!val || 'El usuario es requerido']"
          :disable="loading"
          lazy-rules
          autofocus
        >
          <template v-slot:prepend>
            <q-icon name="person" />
          </template>
        </q-input>

        <!-- Campo Contraseña -->
        <q-input
          v-model="password"
          label="Contraseña"
          :type="showPassword ? 'text' : 'password'"
          outlined
          dense
          :rules="[val => !!val || 'La contraseña es requerida']"
          :disable="loading"
          lazy-rules
          @keyup.enter="onSubmit"
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

        <!-- Mensaje de Error -->
        <q-banner
          v-if="authStore.hasError && authStore.error"
          class="bg-negative text-white q-mt-sm"
          dense
          rounded
        >
          <template v-slot:avatar>
            <q-icon name="error" />
          </template>
          {{ authStore.error }}
        </q-banner>

        <!-- Botón Iniciar Sesión -->
        <div class="q-mt-lg">
          <q-btn
            type="submit"
            label="Iniciar Sesión"
            unelevated
            no-caps
            size="md"
            class="login-btn full-width"
            :loading="loading"
            :disable="loading"
          >
            <template v-slot:loading>
              <q-spinner-hourglass class="on-left" />
              Validando credenciales...
            </template>
          </q-btn>
        </div>
      </q-form>
    </q-card>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from 'src/stores/auth'
import { useQuasar } from 'quasar'

const router = useRouter()
const authStore = useAuthStore()
const $q = useQuasar()

// Verificar que $q esté disponible
if (!$q) {
  console.error('Quasar no está disponible')
}

// Variables reactivas
const usuario = ref('')
const password = ref('')
const showPassword = ref(false)

// Computed para loading
const loading = computed(() => authStore.loading)

// Función para manejar el envío del formulario
const onSubmit = async (evt) => {
  // Prevenir el comportamiento por defecto del formulario
  if (evt) {
    evt.preventDefault()
  }
  
  // Limpiar error previo
  authStore.clearError()

  try {
    console.log('Iniciando login...') // Debug
    const result = await authStore.login(usuario.value, password.value)
    
    console.log('Resultado del login:', result) // Debug
    console.log('result.success:', result?.success) // Debug
    console.log('result:', JSON.stringify(result)) // Debug
    
    // Verificar si el login fue exitoso
    if (result && result.success === true) {
      console.log('✅ Login exitoso, navegando...') // Debug
      
      // Verificar que el token esté guardado
      const token = localStorage.getItem('authToken')
      console.log('Token en localStorage:', token ? 'Sí' : 'No') // Debug
      
      if (!token) {
        console.error('❌ ERROR: No se guardó el token!') // Debug
        return
      }
      
      // Asegurarnos de que el store esté actualizado
      authStore.checkAuth()
      
      // Mostrar notificación de éxito
      $q.notify({
        type: 'positive',
        message: '¡Bienvenido!',
        position: 'top',
        timeout: 1000
      })
      
      // Forzar navegación
      console.log('🌐 Navegando a /#/dashboard...') // Debug
      setTimeout(() => {
        console.log('Ejecutando window.location.href...') // Debug
        window.location.href = '/#/dashboard'
      }, 100)
    } else {
      console.warn('⚠️ Login no exitoso:', result) // Debug
    }
  } catch (error) {
    console.error('❌ Error en login:', error) // Debug
    console.error('Error completo:', JSON.stringify(error, null, 2)) // Debug
  }
}

// Verificar si ya está autenticado al cargar la página
onMounted(() => {
  authStore.checkAuth()
  if (authStore.isAuthenticated) {
    router.push('/dashboard').catch(() => {
      // Fallback: usar window.location si hay error
      window.location.href = '/#/dashboard'
    })
  }
})
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #bdbdbd;
  padding: 16px;
}

.login-card {
  width: 100%;
  max-width: 400px;
  border-radius: 12px;
  background-color: #ffffff;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.18);
  overflow: hidden;
}

.logo-section {
  background-color: #000000;
  padding: 24px;
  margin: -48px -48px 24px -48px;
  border-radius: 0;
}

.login-title {
  color: #ffffff;
}

.login-subtitle {
  color: rgba(255, 255, 255, 0.85);
}

.login-form {
  color: #000000;
}

.login-form :deep(.q-field__label),
.login-form :deep(.q-field__native),
.login-form :deep(.q-icon) {
  color: #000000;
}

.login-form :deep(.q-field--focused .q-field__label),
.login-form :deep(.q-field--focused .q-field__control:before) {
  color: #f44336;
  border-color: #f44336;
}

.login-btn {
  background-color: #f44336 !important;
  color: #ffffff !important;
  border-radius: 8px;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.login-btn:hover {
  background-color: #e53935 !important;
}

.logo-image {
  height: 64px;
  width: auto;
  max-width: 280px;
  object-fit: contain;
  margin-bottom: 16px;
}

/* Responsive: ajustar padding en pantallas pequeñas */
@media (max-width: 600px) {
  .login-container {
    padding: 8px;
  }
  
  .login-card {
    margin: 0;
    padding: 24px !important;
  }
  
  .logo-section {
    margin: -24px -24px 24px -24px;
    padding: 16px;
  }
  
  .logo-image {
    height: 48px;
    max-width: 220px;
  }
}
</style>

