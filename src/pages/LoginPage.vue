<template>
  <div class="login-container">
    <q-card class="login-card q-pa-xl" flat bordered>
      <!-- Logo -->
      <div class="logo-section text-center q-mb-xl">
        <img 
          src="https://bradial.mx/imagenes/logo/logo100.png" 
          alt="Bradial Logo" 
          class="logo-image"
        />
        <div class="text-h4 text-weight-bold text-primary q-mt-md">
          Iniciar Sesión
        </div>
        <div class="text-subtitle2 text-grey-7 q-mt-xs">
          Ingresa tus credenciales para continuar
        </div>
      </div>

      <!-- Formulario de Login -->
      <q-form @submit.prevent="onSubmit" class="q-gutter-md">
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
            color="primary"
            size="md"
            class="full-width"
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
  background-color: #000000;
  background-image: url('https://bradial.mx/imagenes/carrucel/invault.jpg');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  padding: 16px;
  position: relative;
}

.login-container::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.6);
  z-index: 0;
}

.login-container > * {
  position: relative;
  z-index: 1;
}

.login-card {
  width: 100%;
  max-width: 400px;
  border-radius: 12px;
}

.logo-section {
  background-color: #000000;
  padding: 24px;
  margin: -24px -24px 24px -24px;
  border-radius: 12px 12px 0 0;
}

.logo-image {
  height: 80px;
  width: auto;
  max-width: 150px;
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
    height: 60px;
    max-width: 120px;
  }
}
</style>

