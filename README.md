# MesaSitec

MesaSitec es una aplicación web de mesa de servicio multiempresa desarrollada como prueba técnica.

Permite que distintas organizaciones utilicen la misma aplicación y base de datos, manteniendo sus solicitudes, usuarios y categorías completamente aislados mediante el `tenantId` incluido en el token JWT.

## Tecnologías utilizadas

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Bearer
- BCrypt
- Swagger / OpenAPI
- xUnit

### Frontend

- Vue 3
- TypeScript en modo estricto
- Vite
- Vue Router
- Pinia
- CSS

## Requisitos previos

Para ejecutar el proyecto es necesario tener instalado:

- .NET SDK 8.0.423 o una versión compatible de .NET 8
- Node.js 20.19 o superior
- npm
- Git

No es necesario instalar un servidor de base de datos. El proyecto utiliza SQLite mediante un archivo local.

## Cómo levantar el proyecto

Después de clonar el repositorio, ubícate en la carpeta raíz de `MesaSitec`.

El proyecto completo se levanta utilizando dos terminales y dos comandos.

### Terminal 1 — Backend

Desde la raíz del repositorio ejecuta:

```bash
set -a && source .env.example && set +a && dotnet run --project backend/src/MesaSitec.Api
```

Al iniciar el backend:

- Se restauran las dependencias de .NET.
- Se crea automáticamente la base de datos SQLite.
- Se aplican las migraciones pendientes.
- Se insertan los datos semilla cuando la base de datos está vacía.
- Se inicia la API en el puerto `5080`.

El backend quedará disponible en:

- API: http://localhost:5080
- API base: http://localhost:5080/api/v1
- Health: http://localhost:5080/api/v1/health
- Swagger: http://localhost:5080/swagger

La respuesta esperada de `/api/v1/health` es:

```json
{
  "estado": "ok"
}
```

### Terminal 2 — Frontend

Abre otra terminal en la raíz del repositorio y ejecuta:

```bash
cd frontend && npm ci && npm run dev
```

El frontend quedará disponible en:

- http://localhost:5173

## Variables de entorno

El archivo `.env.example` contiene los valores necesarios para ejecutar el proyecto localmente:

```env
JWT_SECRET=MesaSitec.Cambie.Esta.Clave.Local.De.AlMenos32Caracteres
SEED_FECHA_BASE=2026-01-15T08:00:00Z
```

### `JWT_SECRET`

Se utiliza para firmar y validar los tokens JWT.

Debe tener al menos 32 caracteres.

### `SEED_FECHA_BASE`

Define la fecha base utilizada para generar los datos semilla de forma reproducible.

El valor predeterminado es:

```text
2026-01-15T08:00:00Z
```

El archivo `.env` está excluido del repositorio mediante `.gitignore`.

## Credenciales de prueba

Todos los usuarios semilla utilizan la siguiente contraseña:

```text
Sitec.2026
```

### Cooperativa Norte

| Correo | Rol |
|---|---|
| `admin@norte.test` | Admin |
| `agente1@norte.test` | Agente |
| `agente2@norte.test` | Agente |
| `user1@norte.test` | Solicitante |
| `user2@norte.test` | Solicitante |

### Bufete Sur

| Correo | Rol |
|---|---|
| `admin@sur.test` | Admin |
| `user1@sur.test` | Solicitante |

Para una primera prueba se recomienda utilizar:

```text
Correo: agente1@norte.test
Contraseña: Sitec.2026
```

## Roles y permisos

### Administrador

Puede:

- Ver todas las solicitudes de su organización.
- Crear solicitudes.
- Editar solicitudes.
- Asignar y reasignar agentes.
- Iniciar solicitudes.
- Resolver solicitudes.
- Cerrar solicitudes.
- Reabrir solicitudes.
- Cancelar solicitudes.

### Agente

Puede:

- Ver todas las solicitudes de su organización.
- Crear solicitudes.
- Editar solicitudes.
- Asignar y reasignar agentes.
- Iniciar solicitudes.
- Resolver solicitudes.
- Cerrar solicitudes.
- Reabrir solicitudes.

Un agente no puede cancelar solicitudes.

### Solicitante

Puede:

- Ver únicamente las solicitudes que él creó.
- Crear solicitudes.
- Editar sus propias solicitudes cuando estén en estado `Nueva`.
- Cerrar sus propias solicitudes cuando estén en estado `Resuelta`.

No puede asignar, iniciar, resolver, reabrir ni cancelar solicitudes.

## Flujo de estados

El flujo principal de una solicitud es:

```text
Nueva → Asignada → EnProceso → Resuelta → Cerrada
```

También se permiten las siguientes operaciones:

- Reasignar una solicitud en estado `Asignada`.
- Reasignar una solicitud en estado `EnProceso`, regresándola a `Asignada`.
- Reabrir una solicitud `Resuelta`, regresándola a `EnProceso`.
- Cancelar una solicitud `Nueva`, `Asignada` o `EnProceso`.

Los estados `Cerrada` y `Cancelada` son estados finales.

## Cálculo del SLA

Cada categoría define una cantidad base de horas.

La prioridad modifica ese tiempo utilizando los siguientes factores:

| Prioridad | Factor |
|---|---:|
| Crítica | 0.5 |
| Alta | 0.75 |
| Media | 1.0 |
| Baja | 2.0 |

La fecha límite se calcula en el servidor:

```text
fechaLimiteSla = fechaCreacion + (slaHoras × factorPrioridad)
```

Si se modifica la categoría o prioridad de una solicitud que todavía no está resuelta, el SLA se calcula nuevamente sin modificar la fecha de creación.

Una solicitud está vencida cuando su fecha límite ya pasó y no se encuentra en estado `Resuelta`, `Cerrada` o `Cancelada`.

## Endpoints de la API

La URL base es:

```text
http://localhost:5080/api/v1
```

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/auth/login` | Iniciar sesión |
| GET | `/me` | Obtener el perfil autenticado |
| GET | `/categorias` | Listar categorías activas |
| GET | `/solicitudes` | Listar, filtrar, ordenar y paginar solicitudes |
| POST | `/solicitudes` | Crear una solicitud |
| GET | `/solicitudes/{id}` | Obtener el detalle de una solicitud |
| PUT | `/solicitudes/{id}` | Editar una solicitud |
| POST | `/solicitudes/{id}/transiciones` | Ejecutar una transición de estado |
| GET | `/health` | Verificar el estado del servicio |

Todos los endpoints, excepto `/auth/login` y `/health`, requieren un token JWT válido.

## Probar la API con Swagger

1. Abre http://localhost:5080/swagger.
2. Ejecuta `POST /api/v1/auth/login`.
3. Utiliza las siguientes credenciales:

```json
{
  "email": "admin@norte.test",
  "password": "Sitec.2026"
}
```

4. Copia el valor de `accessToken`.
5. Presiona el botón `Authorize`.
6. Ingresa el token.
7. Prueba los endpoints protegidos.

## Datos semilla

La base de datos se llena automáticamente cuando está vacía.

Los datos incluyen:

- 2 organizaciones.
- 7 usuarios.
- 4 categorías por organización.
- 25 solicitudes para Cooperativa Norte.
- 8 solicitudes para Bufete Sur.
- Solicitudes distribuidas entre todos los estados.
- Solicitudes distribuidas entre todas las prioridades.
- Solicitudes vencidas.
- Solicitudes resueltas.

Las organizaciones creadas son:

- Cooperativa Norte.
- Bufete Sur.

Las categorías creadas para ambas organizaciones son:

| Categoría | SLA base |
|---|---:|
| Incidente | 8 horas |
| Requerimiento | 40 horas |
| Consulta | 24 horas |
| Falla crítica | 4 horas |

## Aislamiento entre organizaciones

Todas las consultas utilizan el `tenantId` obtenido desde el token JWT.

Un usuario de Cooperativa Norte no puede consultar ni modificar recursos de Bufete Sur y viceversa.

Cuando un usuario intenta consultar un recurso de otra organización, la API responde:

```text
404 Not Found
```

Esto evita revelar que el recurso existe en otra organización.

## Manejo de errores

Los errores de la API utilizan el formato:

```text
application/problem+json
```

Cada error incluye el campo obligatorio `codigo`.

Algunos códigos utilizados son:

| HTTP | Código |
|---:|---|
| 401 | `NO_AUTENTICADO` |
| 403 | `OPERACION_NO_PERMITIDA` |
| 404 | `RECURSO_NO_ENCONTRADO` |
| 409 | `TRANSICION_INVALIDA` |
| 422 | `AGENTE_INVALIDO` |
| 422 | `MOTIVO_REQUERIDO` |
| 422 | `VALIDACION` |
| 400 | `PARAMETRO_INVALIDO` |

La API también cuenta con un middleware global para evitar enviar trazas de excepciones al cliente.

## Funcionalidades implementadas

### Backend

- Arquitectura separada en API, Aplicación, Dominio e Infraestructura.
- Autenticación JWT con expiración.
- Contraseñas almacenadas utilizando BCrypt.
- Aislamiento de datos por organización.
- Migraciones automáticas.
- Base de datos SQLite.
- Datos semilla reproducibles.
- Consulta del perfil autenticado.
- Listado de categorías.
- Creación de solicitudes.
- Edición de solicitudes.
- Detalle de solicitudes.
- Listado paginado.
- Filtros ejecutados en el servidor.
- Búsqueda ejecutada en el servidor.
- Ordenamiento ejecutado en el servidor.
- Cálculo automático del SLA.
- Máquina de estados.
- Validaciones de permisos por rol.
- Manejo uniforme de errores.
- Swagger con autenticación Bearer.
- CORS habilitado para el frontend.
- Pruebas unitarias con xUnit.

### Frontend

- Inicio de sesión.
- Rutas privadas protegidas.
- Almacenamiento de sesión mediante Pinia.
- Cliente HTTP centralizado.
- Inclusión automática del token JWT.
- Redirección al login cuando la sesión expira.
- Listado de solicitudes.
- Filtros.
- Búsqueda.
- Paginación.
- Creación de solicitudes.
- Edición de solicitudes.
- Vista de detalle.
- Ejecución de transiciones.
- Validaciones del formulario.
- Manejo de estados de carga.
- Manejo de estados vacíos.
- Manejo de errores.
- Botones renderizados según el rol y el estado.
- Atributos `data-testid` requeridos para pruebas automáticas.

## Funcionalidades no incluidas

No se incluyó Docker Compose porque es una característica opcional y el proyecto puede levantarse directamente con .NET y Node.js en dos comandos.

Tampoco se incluyó:

- Despliegue en un proveedor de nube.
- Integración continua.
- Generación automática de DTOs TypeScript desde OpenAPI.
- Un endpoint adicional para listar agentes.

Estas funcionalidades no forman parte del contrato obligatorio de los nueve endpoints.

## Ejecutar las pruebas del backend

Desde la raíz del repositorio:

```bash
dotnet test backend/MesaSitec.sln
```

Las pruebas unitarias cubren:

- Máquina de estados.
- Cálculo del SLA.
- Permisos por rol.

## Verificar el frontend

Desde la raíz del repositorio:

```bash
cd frontend && npm run type-check && npm run build
```

El comando debe finalizar sin errores de TypeScript y generar la aplicación de producción en la carpeta `frontend/dist`.

## Estructura del proyecto

```text
MesaSitec/
├── backend/
│   ├── src/
│   │   ├── MesaSitec.Api/
│   │   ├── MesaSitec.Aplicacion/
│   │   ├── MesaSitec.Dominio/
│   │   └── MesaSitec.Infraestructura/
│   └── tests/
│       └── MesaSitec.UnitTests/
├── frontend/
│   └── src/
│       ├── api/
│       ├── components/
│       ├── router/
│       ├── stores/
│       ├── types/
│       └── views/
├── .env.example
├── .gitignore
├── DECISIONES.md
├── global.json
└── README.md
```

## Documentación adicional

Las decisiones técnicas, las alternativas descartadas y el uso de herramientas de inteligencia artificial se encuentran documentados en:

```text
DECISIONES.md
```

## Autor

Samahel Thomas