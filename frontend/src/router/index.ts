import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import SolicitudesView from '../views/SolicitudesView.vue'
import SolicitudFormularioView from '../views/SolicitudFormularioView.vue'
import SolicitudDetalleView from '../views/SolicitudDetalleView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/solicitudes' },
    { path: '/login', component: LoginView, meta: { publica: true } },
    { path: '/solicitudes', component: SolicitudesView },
    { path: '/solicitudes/nueva', component: SolicitudFormularioView },
    { path: '/solicitudes/:id', component: SolicitudDetalleView },
    { path: '/solicitudes/:id/editar', component: SolicitudFormularioView },
  ],
})

router.beforeEach((destino) => {
  const autenticado = Boolean(localStorage.getItem('token'))
  if (!destino.meta.publica && !autenticado) return '/login'
  if (destino.path === '/login' && autenticado) return '/solicitudes'
})

export default router
