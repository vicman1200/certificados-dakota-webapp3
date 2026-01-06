# PASO 10: Verificación y Endpoint Protegido

## Objetivo
Verificar que el sistema de autenticación funciona correctamente y crear un endpoint protegido para probar que el token JWT se usa correctamente.

---

## Paso 10.1: Verificar que el Login Funciona

### Prueba 1: Login Exitoso

1. **Abre tu navegador** y ve a: `http://localhost:8080/#/login`

2. **Ingresa las credenciales:**
   - Usuario: `admin`
   - Contraseña: `admin123`

3. **Haz clic en "Iniciar Sesión"**

4. **Verifica:**
   - ✅ El botón muestra "Validando credenciales..."
   - ✅ Se muestra una notificación de éxito
   - ✅ Se redirige al dashboard
   - ✅ No hay errores en la consola del navegador (F12)

### Prueba 2: Verificar el Token en localStorage

1. **Abre las DevTools** (F12)
2. **Ve a la pestaña Application** (Aplicación)
3. **En el menú lateral**, ve a **Local Storage** → `http://localhost:8080`
4. **Busca la clave** `authToken`
5. **Verifica:**
   - ✅ Debe existir un token JWT largo
   - ✅ Copia el token para verificar más adelante

### Prueba 3: Verificar en la Consola de la API

1. **Ve a la terminal donde corre la API**
2. **Verifica que aparezca:**
   - `Login exitoso para el usuario: admin`
   - No debe haber errores

---

## Paso 10.2: Crear un Endpoint Protegido

Vamos a crear un endpoint que requiera autenticación para probar que el token JWT funciona correctamente.

### Código para el Endpoint Protegido

Crea un nuevo archivo en `Controllers/UsuarioController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using bradial_webapi.Models;

namespace bradial_webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la información del usuario actual autenticado
        /// </summary>
        /// <returns>Información del usuario</returns>
        /// <response code="200">Usuario autenticado</response>
        /// <response code="401">No autenticado</response>
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(UsuarioInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetPerfil()
        {
            try
            {
                // Obtener información del usuario desde los claims del token
                var usuario = User.Identity?.Name;
                var nombre = User.FindFirst("nombre")?.Value ?? usuario ?? "Usuario";
                var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Usuario";
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

                var usuarioInfo = new UsuarioInfo
                {
                    Usuario = usuario ?? string.Empty,
                    Nombre = nombre,
                    Rol = rol,
                    Email = email
                };

                _logger.LogInformation("Usuario {Usuario} consultó su perfil", usuario);

                return Ok(usuarioInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Mensaje = "Error al obtener el perfil",
                    Detalle = ex.Message,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Verifica si el token es válido
        /// </summary>
        /// <returns>Estado del token</returns>
        [HttpGet("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult VerifyToken()
        {
            var usuario = User.Identity?.Name;
            
            return Ok(new
            {
                mensaje = "Token válido",
                usuario = usuario,
                autenticado = User.Identity?.IsAuthenticated ?? false,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}
```

---

## Paso 10.3: Actualizar el Frontend para Probar el Endpoint

Vamos a actualizar el Dashboard para que muestre información del usuario autenticado.

### Actualizar DashboardPage.vue

Actualiza `src/pages/DashboardPage.vue`:

```vue
<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-card class="q-pa-md">
          <q-card-section>
            <div class="text-h5">Dashboard</div>
            <div class="text-subtitle2 text-grey-7">
              Bienvenido, {{ usuarioInfo?.nombre || 'Usuario' }}
            </div>
          </q-card-section>
          
          <q-card-section v-if="loading">
            <q-spinner color="primary" size="3em" />
            <div class="q-mt-md">Cargando información del usuario...</div>
          </q-card-section>

          <q-card-section v-else-if="usuarioInfo">
            <div class="q-gutter-md">
              <div>
                <strong>Usuario:</strong> {{ usuarioInfo.usuario }}
              </div>
              <div>
                <strong>Nombre:</strong> {{ usuarioInfo.nombre }}
              </div>
              <div>
                <strong>Rol:</strong> {{ usuarioInfo.rol }}
              </div>
              <div>
                <strong>Email:</strong> {{ usuarioInfo.email }}
              </div>
            </div>
          </q-card-section>

          <q-card-section>
            <q-btn
              color="negative"
              label="Cerrar Sesión"
              icon="logout"
              @click="handleLogout"
            />
          </q-card-section>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from 'src/stores/auth'
import { useQuasar } from 'quasar'
import api from 'src/services/api'

const router = useRouter()
const authStore = useAuthStore()
const $q = useQuasar()

const usuarioInfo = ref(null)
const loading = ref(false)

const cargarPerfil = async () => {
  loading.value = true
  try {
    const response = await api.get('/usuario/perfil')
    usuarioInfo.value = response.data
  } catch (error) {
    console.error('Error al cargar perfil:', error)
    if (error.response?.status === 401) {
      $q.notify({
        type: 'negative',
        message: 'Sesión expirada. Por favor, inicia sesión nuevamente.',
        position: 'top'
      })
      authStore.logout()
      router.push('/login')
    }
  } finally {
    loading.value = false
  }
}

const handleLogout = () => {
  authStore.logout()
  $q.notify({
    type: 'info',
    message: 'Sesión cerrada correctamente',
    position: 'top'
  })
  router.push('/login')
}

// Verificar autenticación al cargar
onMounted(async () => {
  authStore.checkAuth()
  if (!authStore.isAuthenticated) {
    router.push('/login')
  } else {
    await cargarPerfil()
  }
})
</script>
```

---

## Paso 10.4: Probar el Endpoint Protegido

### Prueba 1: Sin Token (debe fallar)

1. **Abre una nueva pestaña** en el navegador
2. **Ve a:** `http://localhost:5045/api/usuario/perfil`
3. **Verifica:**
   - ❌ Debe retornar `401 Unauthorized`
   - ✅ Esto confirma que el endpoint está protegido

### Prueba 2: Con Token (debe funcionar)

1. **Abre las DevTools** (F12)
2. **Ve a la pestaña Console**
3. **Ejecuta este código:**
```javascript
const token = localStorage.getItem('authToken');
fetch('http://localhost:5045/api/usuario/perfil', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(r => r.json())
.then(data => console.log('Perfil:', data))
.catch(err => console.error('Error:', err));
```

4. **Verifica:**
   - ✅ Debe retornar la información del usuario
   - ✅ Debe mostrar: `{ usuario: "admin", nombre: "Administrador", ... }`

### Prueba 3: Desde el Dashboard

1. **Ve al dashboard** después de hacer login
2. **Verifica:**
   - ✅ Debe mostrar la información del usuario
   - ✅ Debe cargar sin errores
   - ✅ Si hay error 401, debe redirigir al login

---

## Paso 10.5: Verificar en Swagger

1. **Abre Swagger** en tu navegador: `http://localhost:5045/swagger`

2. **Busca el endpoint** `GET /api/usuario/perfil`

3. **Haz clic en "Try it out"**

4. **Sin autenticación:**
   - Haz clic en "Execute"
   - Debe retornar `401 Unauthorized`

5. **Con autenticación:**
   - Haz clic en el botón "Authorize" (🔒) en la parte superior
   - Ingresa el token JWT (sin la palabra "Bearer")
   - Haz clic en "Authorize"
   - Ahora ejecuta el endpoint
   - Debe retornar `200 OK` con la información del usuario

---

## ✅ Verificación Final

Si todo funciona correctamente:

1. ✅ El login funciona y guarda el token
2. ✅ El dashboard carga la información del usuario
3. ✅ El endpoint protegido funciona con el token
4. ✅ El endpoint protegido rechaza peticiones sin token
5. ✅ El token se incluye automáticamente en las peticiones
6. ✅ Si el token expira, se redirige al login

---

## Próximos Pasos

Una vez que todo funcione:

1. ✅ Sistema de autenticación completo
2. ✅ Endpoints protegidos funcionando
3. ✅ Frontend conectado correctamente
4. ✅ Listo para agregar más funcionalidades

