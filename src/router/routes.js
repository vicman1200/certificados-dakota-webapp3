
const routes = [
  {
    path: '/login',
    component: () => import('pages/LoginPage.vue')
  },
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      { 
        path: '', 
        redirect: '/dashboard'
      },
      { 
        path: 'dashboard', 
        component: () => import('pages/DashboardPage.vue'),
        meta: { requiresAuth: true }
      },
      { 
        path: 'usuario', 
        component: () => import('pages/UsuarioPage.vue'),
        meta: { requiresAuth: true }
      },
      { 
        path: 'index', 
        component: () => import('pages/IndexPage.vue'),
        meta: { requiresAuth: true }
      }
    ]
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue')
  }
]

export default routes
