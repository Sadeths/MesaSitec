import type { ProblemaApi } from '../types/api'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api/v1'

export class ErrorApi extends Error {
  readonly problema: ProblemaApi
  readonly estado: number

  constructor(problema: ProblemaApi, estado: number) {
    super(problema.detail)
    this.problema = problema
    this.estado = estado
  }
}

export async function peticionApi<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('token')
  const headers = new Headers(opciones.headers)
  headers.set('Accept', 'application/json')

  if (opciones.body) headers.set('Content-Type', 'application/json')
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const respuesta = await fetch(`${API_URL}${ruta}`, { ...opciones, headers })

  if (respuesta.status === 401 && ruta !== '/auth/login') {
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
    window.location.assign('/login')
  }

  if (!respuesta.ok) {
    const problema = (await respuesta.json()) as ProblemaApi
    throw new ErrorApi(problema, respuesta.status)
  }

  return (await respuesta.json()) as T
}
