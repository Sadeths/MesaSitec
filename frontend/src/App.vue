<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAutenticacionStore } from './stores/autenticacion'

const autenticacion = useAutenticacionStore()
const router = useRouter()

function salir(): void {
  autenticacion.logout()
  void router.push('/login')
}
</script>

<template>
  <header v-if="autenticacion.usuario" class="barra" data-testid="app-nav">
    <RouterLink class="marca" to="/solicitudes">MesaSitec</RouterLink>
    <div class="usuario-nav">
      <span data-testid="nav-usuario-nombre">{{ autenticacion.usuario.nombre }}</span>
      <span class="rol" data-testid="nav-usuario-rol">{{ autenticacion.usuario.rol }}</span>
      <button class="boton secundario" data-testid="btn-logout" @click="salir">Cerrar sesión</button>
    </div>
  </header>
  <main class="contenedor">
    <RouterView />
  </main>
  <div class="toast" data-testid="toast-mensaje" aria-live="polite"></div>
</template>
