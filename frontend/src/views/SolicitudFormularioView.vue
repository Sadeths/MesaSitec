<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ErrorApi, peticionApi } from '../api/clienteHttp'
import SolicitudFormulario from '../components/SolicitudFormulario.vue'
import type { Categoria, SolicitudDetalle, SolicitudPeticion } from '../types/api'

const route = useRoute()
const router = useRouter()
const id = computed(() => typeof route.params.id === 'string' ? route.params.id : '')
const editando = computed(() => Boolean(id.value))
const categorias = ref<Categoria[]>([])
const inicial = ref<SolicitudPeticion | undefined>()
const cargando = ref(true)
const enviando = ref(false)
const error = ref('')
const errores = ref<Record<string, string[]>>({})

onMounted(async () => {
  try {
    categorias.value = await peticionApi<Categoria[]>('/categorias')
    if (editando.value) {
      const solicitud = await peticionApi<SolicitudDetalle>(`/solicitudes/${id.value}`)
      inicial.value = { titulo: solicitud.titulo, descripcion: solicitud.descripcion, categoriaId: solicitud.categoria.id, prioridad: solicitud.prioridad }
    }
  } catch { error.value = 'No fue posible cargar el formulario.' }
  finally { cargando.value = false }
})

async function guardar(datos: SolicitudPeticion): Promise<void> {
  enviando.value = true
  errores.value = {}
  try {
    const ruta = editando.value ? `/solicitudes/${id.value}` : '/solicitudes'
    const solicitud = await peticionApi<SolicitudDetalle>(ruta, { method: editando.value ? 'PUT' : 'POST', body: JSON.stringify(datos) })
    await router.push(`/solicitudes/${solicitud.id}`)
  } catch (excepcion: unknown) {
    if (excepcion instanceof ErrorApi) {
      errores.value = excepcion.problema.errores ?? {}
      error.value = excepcion.problema.detail
    } else error.value = 'No fue posible guardar la solicitud.'
  } finally { enviando.value = false }
}
</script>

<template>
  <section class="seccion-angosta">
    <div class="encabezado-pagina"><div><p class="eyebrow">Solicitudes</p><h1>{{ editando ? 'Editar solicitud' : 'Nueva solicitud' }}</h1></div></div>
    <div v-if="cargando" class="estado">Cargando formulario…</div>
    <p v-else-if="error && !inicial && editando" class="alerta error">{{ error }}</p>
    <template v-else>
      <p v-if="error" class="alerta error">{{ error }}</p>
      <SolicitudFormulario :key="id" :categorias="categorias" :inicial="inicial" :enviando="enviando" :errores-servidor="errores" @guardar="guardar" @cancelar="router.back()" />
    </template>
  </section>
</template>
