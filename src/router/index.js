import { route } from 'quasar/wrappers'
import { createRouter, createMemoryHistory, createWebHistory, createWebHashHistory } from 'vue-router'
import routes from './routes'

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

export default route(function ({ store }) {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : (process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory)

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createHistory(process.env.MODE === 'ssr' ? void 0 : process.env.VUE_ROUTER_BASE)
  })

  // Guard de navegación para proteger rutas que requieren autenticación
  Router.beforeEach((to, from, next) => {
    // Importar el store de autenticación
    const { useAuthStore } = require('src/stores/auth')
    const authStore = useAuthStore()
    
    // Siempre verificar el estado de autenticación antes de cada navegación
    authStore.checkAuth()
    
    // Verificar si la ruta requiere autenticación
    if (to.matched.some(record => record.meta.requiresAuth)) {
      // Verificar si el usuario está autenticado (tanto en store como en localStorage)
      const hasToken = localStorage.getItem('authToken')
      if (!authStore.isAuthenticated && !hasToken) {
        // Redirigir a login si no está autenticado
        next({ path: '/login', query: { redirect: to.fullPath } })
      } else {
        // Si hay token pero el store no está actualizado, actualizarlo
        if (hasToken && !authStore.isAuthenticated) {
          authStore.checkAuth()
        }
        next()
      }
    } else if (to.path === '/login' && (authStore.isAuthenticated || localStorage.getItem('authToken'))) {
      // Si ya está autenticado y va a login, redirigir al dashboard
      next({ path: '/dashboard' })
    } else {
      next()
    }
  })

  return Router
})
