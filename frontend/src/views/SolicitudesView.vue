<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { peticionApi } from '../api/clienteHttp'
import type { Categoria, PaginaSolicitudes } from '../types/api'

const router = useRouter()
const categorias = ref<Categoria[]>([])
const pagina = ref<PaginaSolicitudes>({ items: [], page: 1, pageSize: 20, total: 0, totalPaginas: 0 })
const cargando = ref(true)
const error = ref('')
const filtros = reactive({ estado: '', prioridad: '', categoriaId: '', vencidas: '', q: '', page: 1 })
let temporizador: ReturnType<typeof setTimeout> | undefined

async function cargar(): Promise<void> {
  cargando.value = true
  error.value = ''
  const parametros = new URLSearchParams({ page: String(filtros.page), pageSize: '20' })
  Object.entries(filtros).forEach(([clave, valor]) => {
    if (clave !== 'page' && valor !== '') parametros.set(clave, String(valor))
  })
  try {
    pagina.value = await peticionApi<PaginaSolicitudes>(`/solicitudes?${parametros}`)
  } catch {
    error.value = 'No fue posible cargar las solicitudes.'
  } finally {
    cargando.value = false
  }
}

function cambiarFiltro(): void {
  filtros.page = 1
  void cargar()
}

function buscar(): void {
  clearTimeout(temporizador)
  temporizador = setTimeout(cambiarFiltro, 350)
}

function limpiar(): void {
  Object.assign(filtros, { estado: '', prioridad: '', categoriaId: '', vencidas: '', q: '', page: 1 })
  void cargar()
}

watch(() => filtros.page, cargar)
onMounted(async () => {
  try { categorias.value = await peticionApi<Categoria[]>('/categorias') } catch { categorias.value = [] }
  await cargar()
})

function fecha(valor: string): string {
  return new Intl.DateTimeFormat('es-GT', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(valor))
}
</script>

<template>
  <section>
    <div class="encabezado-pagina">
      <div><p class="eyebrow">Gestión</p><h1>Solicitudes</h1></div>
      <button class="boton primario" data-testid="btn-nueva-solicitud" @click="router.push('/solicitudes/nueva')">Nueva solicitud</button>
    </div>
    <div class="tarjeta filtros">
      <input v-model="filtros.q" data-testid="filtro-busqueda" placeholder="Buscar por código, título o descripción" @input="buscar" />
      <select v-model="filtros.estado" data-testid="filtro-estado" @change="cambiarFiltro"><option value="">Todos los estados</option><option v-for="e in ['Nueva','Asignada','EnProceso','Resuelta','Cerrada','Cancelada']" :key="e">{{ e }}</option></select>
      <select v-model="filtros.prioridad" data-testid="filtro-prioridad" @change="cambiarFiltro"><option value="">Todas las prioridades</option><option v-for="p in ['Baja','Media','Alta','Critica']" :key="p">{{ p }}</option></select>
      <select v-model="filtros.categoriaId" data-testid="filtro-categoria" @change="cambiarFiltro"><option value="">Todas las categorías</option><option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option></select>
      <select v-model="filtros.vencidas" data-testid="filtro-vencidas" @change="cambiarFiltro"><option value="">Todas</option><option value="true">Solo vencidas</option><option value="false">No vencidas</option></select>
      <button class="boton secundario" data-testid="btn-limpiar-filtros" @click="limpiar">Limpiar</button>
    </div>
    <div v-if="cargando" class="estado" data-testid="listado-cargando">Cargando solicitudes…</div>
    <div v-else-if="error" class="estado error">{{ error }} <button class="boton secundario" @click="cargar">Reintentar</button></div>
    <div v-else-if="pagina.items.length === 0" class="estado" data-testid="listado-vacio">No hay solicitudes para mostrar.</div>
    <div v-else class="tabla-contenedor tarjeta">
      <table data-testid="tabla-solicitudes">
        <thead><tr><th>Código</th><th>Título</th><th>Estado</th><th>Prioridad</th><th>Categoría</th><th>SLA</th></tr></thead>
        <tbody><tr v-for="item in pagina.items" :key="item.id" data-testid="fila-solicitud" :data-codigo="item.codigo" @click="router.push(`/solicitudes/${item.id}`)">
          <td data-testid="celda-codigo"><strong>{{ item.codigo }}</strong></td><td>{{ item.titulo }}</td>
          <td data-testid="celda-estado"><span class="badge">{{ item.estado }}</span></td><td data-testid="celda-prioridad">{{ item.prioridad }}</td><td>{{ item.categoria.nombre }}</td>
          <td data-testid="celda-sla">{{ fecha(item.fechaLimiteSla) }} <span v-if="item.vencida" class="badge peligro" data-testid="badge-vencida">Vencida</span></td>
        </tr></tbody>
      </table>
    </div>
    <div v-if="!cargando" class="paginacion">
      <button class="boton secundario" data-testid="paginacion-anterior" :disabled="pagina.page <= 1" @click="filtros.page--">Anterior</button>
      <span data-testid="paginacion-info">Página {{ pagina.page }} de {{ pagina.totalPaginas }} — {{ pagina.total }} resultados</span>
      <button class="boton secundario" data-testid="paginacion-siguiente" :disabled="pagina.page >= pagina.totalPaginas" @click="filtros.page++">Siguiente</button>
    </div>
  </section>
</template>
