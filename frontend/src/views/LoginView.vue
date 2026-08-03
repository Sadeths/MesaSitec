<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ErrorApi } from '../api/clienteHttp'
import { useAutenticacionStore } from '../stores/autenticacion'

const email = ref('agente1@norte.test')
const password = ref('Sitec.2026')
const cargando = ref(false)
const error = ref('')
const autenticacion = useAutenticacionStore()
const router = useRouter()

async function ingresar(): Promise<void> {
  cargando.value = true
  error.value = ''
  try {
    await autenticacion.login(email.value, password.value)
    await router.push('/solicitudes')
  } catch (excepcion: unknown) {
    error.value = excepcion instanceof ErrorApi ? excepcion.problema.detail : 'No fue posible iniciar sesión.'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <section class="login">
    <form class="tarjeta login-tarjeta" @submit.prevent="ingresar">
      <p class="eyebrow">Mesa de servicio</p>
      <h1>Bienvenido a MesaSitec</h1>
      <p class="texto-suave">Ingresa con tu cuenta para administrar solicitudes.</p>
      <label>Correo<input v-model="email" data-testid="login-email" type="email" autocomplete="email" required /></label>
      <label>Contraseña<input v-model="password" data-testid="login-password" type="password" autocomplete="current-password" required /></label>
      <p v-if="error" class="alerta error" data-testid="login-error">{{ error }}</p>
      <button class="boton primario ancho" data-testid="login-submit" :disabled="cargando">
        {{ cargando ? 'Ingresando…' : 'Ingresar' }}
      </button>
    </form>
  </section>
</template>
