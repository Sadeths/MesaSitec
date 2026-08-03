import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { peticionApi } from '../api/clienteHttp'
import type { LoginRespuesta, Usuario } from '../types/api'

export const useAutenticacionStore = defineStore('autenticacion', () => {
  const usuario = ref<Usuario | null>(leerUsuario())
  const autenticado = computed(() => Boolean(localStorage.getItem('token')))

  async function login(email: string, password: string): Promise<void> {
    const respuesta = await peticionApi<LoginRespuesta>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
    localStorage.setItem('token', respuesta.accessToken)
    localStorage.setItem('usuario', JSON.stringify(respuesta.usuario))
    usuario.value = respuesta.usuario
  }

  function logout(): void {
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
    usuario.value = null
  }

  return { usuario, autenticado, login, logout }
})

function leerUsuario(): Usuario | null {
  const valor = localStorage.getItem('usuario')
  if (!valor) return null
  try {
    return JSON.parse(valor) as Usuario
  } catch {
    localStorage.removeItem('usuario')
    return null
  }
}
