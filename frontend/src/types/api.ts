export type Rol = 'Admin' | 'Agente' | 'Solicitante'
export type Estado = 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'
export type Prioridad = 'Baja' | 'Media' | 'Alta' | 'Critica'

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: Rol
  tenantId: string
  tenantNombre: string
}

export interface LoginRespuesta {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface Resumen {
  id: string
  nombre: string
}

export interface SolicitudListadoItem {
  id: string
  codigo: string
  titulo: string
  estado: Estado
  prioridad: Prioridad
  categoria: Resumen
  agente: Resumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalle extends SolicitudListadoItem {
  descripcion: string
  solicitante: Resumen
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface PaginaSolicitudes {
  items: SolicitudListadoItem[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface SolicitudPeticion {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: Prioridad
}

export interface ProblemaApi {
  title: string
  detail: string
  codigo: string
  errores?: Record<string, string[]>
}
