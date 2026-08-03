<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ErrorApi, peticionApi } from '../api/clienteHttp'
import { useAutenticacionStore } from '../stores/autenticacion'
import type { Estado, SolicitudDetalle } from '../types/api'

type Accion = 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'

const route = useRoute()
const router = useRouter()
const autenticacion = useAutenticacionStore()
const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(true)
const error = ref('')
const modal = ref(false)
const accion = ref<Accion>('iniciar')
const agenteId = ref('')
const motivo = ref('')
const errorModal = ref('')
const enviando = ref(false)

const agentesSemilla = computed(() => {
  if (autenticacion.usuario?.tenantId === '11111111-1111-1111-1111-111111111111') {
    return [
      { id: '10000000-0000-0000-0001-000000000001', nombre: 'Administrador Norte' },
      { id: '10000000-0000-0000-0001-000000000002', nombre: 'Agente Uno Norte' },
      { id: '10000000-0000-0000-0001-000000000003', nombre: 'Agente Dos Norte' },
    ]
  }
  return [{ id: '10000000-0000-0000-0002-000000000001', nombre: 'Administrador Sur' }]
})

const accionesEstado: Record<Estado, Accion[]> = {
  Nueva: ['asignar', 'cancelar'], Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'], Resuelta: ['cerrar', 'reabrir'], Cerrada: [], Cancelada: [],
}

const acciones = computed<Accion[]>(() => {
  if (!solicitud.value || !autenticacion.usuario) return []
  const permitidas = accionesEstado[solicitud.value.estado]
  if (autenticacion.usuario.rol === 'Admin') return permitidas
  if (autenticacion.usuario.rol === 'Agente') return permitidas.filter(a => a !== 'cancelar')
  return permitidas.filter(a => a === 'cerrar' && solicitud.value?.solicitante.id === autenticacion.usuario?.id)
})

const puedeEditar = computed(() => {
  if (!solicitud.value || !autenticacion.usuario) return false
  return autenticacion.usuario.rol !== 'Solicitante' ||
    (solicitud.value.estado === 'Nueva' && solicitud.value.solicitante.id === autenticacion.usuario.id)
})

async function cargar(): Promise<void> {
  cargando.value = true
  try { solicitud.value = await peticionApi<SolicitudDetalle>(`/solicitudes/${String(route.params.id)}`) }
  catch { error.value = 'No fue posible cargar la solicitud.' }
  finally { cargando.value = false }
}

function abrirModal(valor: Accion): void {
  accion.value = valor; agenteId.value = ''; motivo.value = ''; errorModal.value = ''; modal.value = true
}

async function confirmar(): Promise<void> {
  enviando.value = true; errorModal.value = ''
  const cuerpo: { accion: Accion; agenteId?: string; motivo?: string } = { accion: accion.value }
  if (accion.value === 'asignar') cuerpo.agenteId = agenteId.value
  if (accion.value === 'resolver' || accion.value === 'cancelar') cuerpo.motivo = motivo.value
  try {
    solicitud.value = await peticionApi<SolicitudDetalle>(`/solicitudes/${String(route.params.id)}/transiciones`, { method: 'POST', body: JSON.stringify(cuerpo) })
    modal.value = false
  } catch (excepcion: unknown) {
    errorModal.value = excepcion instanceof ErrorApi ? excepcion.problema.detail : 'No fue posible ejecutar la acción.'
  } finally { enviando.value = false }
}

function fecha(valor: string): string {
  return new Intl.DateTimeFormat('es-GT', { dateStyle: 'long', timeStyle: 'short' }).format(new Date(valor))
}

onMounted(cargar)
</script>

<template>
  <section class="seccion-angosta">
    <div v-if="cargando" class="estado">Cargando detalle…</div>
    <div v-else-if="error || !solicitud" class="estado error">{{ error }}</div>
    <template v-else>
      <div class="encabezado-pagina"><div><p class="eyebrow" data-testid="detalle-codigo">{{ solicitud.codigo }}</p><h1 data-testid="detalle-titulo">{{ solicitud.titulo }}</h1></div>
        <button v-if="puedeEditar" class="boton secundario" data-testid="btn-editar" @click="router.push(`/solicitudes/${solicitud.id}/editar`)">Editar</button>
      </div>
      <div class="tarjeta detalle">
        <div class="detalle-grid">
          <div><small>Estado</small><strong data-testid="detalle-estado">{{ solicitud.estado }}</strong></div>
          <div><small>Prioridad</small><strong data-testid="detalle-prioridad">{{ solicitud.prioridad }}</strong></div>
          <div><small>Categoría</small><strong data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</strong></div>
          <div><small>Agente</small><strong data-testid="detalle-agente">{{ solicitud.agente?.nombre ?? 'Sin asignar' }}</strong></div>
          <div><small>Creada</small><strong data-testid="detalle-fecha-creacion">{{ fecha(solicitud.fechaCreacion) }}</strong></div>
          <div><small>Límite SLA</small><strong data-testid="detalle-fecha-limite">{{ fecha(solicitud.fechaLimiteSla) }}</strong></div>
        </div>
        <span v-if="solicitud.vencida" class="badge peligro" data-testid="detalle-vencida">SLA vencido</span>
        <hr /><h2>Descripción</h2><p class="descripcion" data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>
        <div v-if="solicitud.motivoResolucion || solicitud.motivoCancelacion" class="alerta" data-testid="detalle-motivo">{{ solicitud.motivoResolucion || solicitud.motivoCancelacion }}</div>
        <div class="acciones-formulario">
          <button v-for="item in acciones" :key="item" class="boton primario" :data-testid="`btn-accion-${item}`" @click="abrirModal(item)">{{ item }}</button>
        </div>
      </div>
    </template>
    <div v-if="modal" class="modal-fondo" @click.self="modal = false">
      <div class="tarjeta modal" data-testid="modal-accion">
        <h2>Confirmar: {{ accion }}</h2>
        <label v-if="accion === 'asignar'">Agente<select v-model="agenteId" data-testid="modal-select-agente"><option value="">Seleccione un agente</option><option v-for="agente in agentesSemilla" :key="agente.id" :value="agente.id">{{ agente.nombre }}</option></select></label>
        <label v-if="accion === 'resolver' || accion === 'cancelar'">Motivo<textarea v-model="motivo" data-testid="modal-motivo" rows="4"></textarea></label>
        <p v-if="errorModal" class="alerta error" data-testid="modal-error">{{ errorModal }}</p>
        <div class="acciones-formulario"><button class="boton primario" data-testid="modal-confirmar" :disabled="enviando" @click="confirmar">Confirmar</button><button class="boton secundario" data-testid="modal-cancelar" @click="modal = false">Cancelar</button></div>
      </div>
    </div>
  </section>
</template>
