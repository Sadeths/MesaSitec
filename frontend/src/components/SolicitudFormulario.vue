<script setup lang="ts">
import { reactive } from 'vue'
import type { Categoria, Prioridad, SolicitudPeticion } from '../types/api'

const props = defineProps<{
  categorias: Categoria[]
  inicial?: SolicitudPeticion
  enviando: boolean
  erroresServidor?: Record<string, string[]>
}>()

const emit = defineEmits<{
  guardar: [datos: SolicitudPeticion]
  cancelar: []
}>()

const formulario = reactive<SolicitudPeticion>({
  titulo: props.inicial?.titulo ?? '',
  descripcion: props.inicial?.descripcion ?? '',
  categoriaId: props.inicial?.categoriaId ?? '',
  prioridad: props.inicial?.prioridad ?? 'Media',
})
const errores = reactive<Record<string, string>>({})

function guardar(): void {
  errores.titulo = formulario.titulo.trim().length < 5 ? 'El título debe tener al menos 5 caracteres.' : ''
  errores.descripcion = formulario.descripcion.trim().length < 10 ? 'La descripción debe tener al menos 10 caracteres.' : ''
  errores.categoriaId = formulario.categoriaId ? '' : 'Seleccione una categoría.'
  if (Object.values(errores).some(Boolean)) return
  emit('guardar', { ...formulario })
}
</script>

<template>
  <form class="tarjeta formulario" @submit.prevent="guardar">
    <label>
      Título
      <input v-model="formulario.titulo" data-testid="form-titulo" maxlength="120" />
      <small v-if="errores.titulo || erroresServidor?.titulo" class="error" data-testid="error-titulo">
        {{ errores.titulo || erroresServidor?.titulo?.[0] }}
      </small>
    </label>
    <label>
      Descripción
      <textarea v-model="formulario.descripcion" data-testid="form-descripcion" rows="7" maxlength="4000"></textarea>
      <small v-if="errores.descripcion || erroresServidor?.descripcion" class="error" data-testid="error-descripcion">
        {{ errores.descripcion || erroresServidor?.descripcion?.[0] }}
      </small>
    </label>
    <div class="columnas">
      <label>
        Categoría
        <select v-model="formulario.categoriaId" data-testid="form-categoria">
          <option value="">Seleccione una categoría</option>
          <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">
            {{ categoria.nombre }} ({{ categoria.slaHoras }} h)
          </option>
        </select>
        <small v-if="errores.categoriaId || erroresServidor?.categoriaId" class="error" data-testid="error-categoria">
          {{ errores.categoriaId || erroresServidor?.categoriaId?.[0] }}
        </small>
      </label>
      <label>
        Prioridad
        <select v-model="formulario.prioridad" data-testid="form-prioridad">
          <option v-for="prioridad in (['Baja', 'Media', 'Alta', 'Critica'] as Prioridad[])" :key="prioridad">
            {{ prioridad }}
          </option>
        </select>
      </label>
    </div>
    <div class="acciones-formulario">
      <button class="boton primario" data-testid="form-submit" :disabled="enviando">
        {{ enviando ? 'Guardando…' : 'Guardar solicitud' }}
      </button>
      <button type="button" class="boton secundario" data-testid="form-cancelar" @click="emit('cancelar')">Cancelar</button>
    </div>
  </form>
</template>
