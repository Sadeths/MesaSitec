# MesaSitec

Mesa de servicio multiempresa desarrollada con ASP.NET Core, Entity Framework
Core, SQLite, JWT y Vue 3.

## Requisitos

- .NET SDK 8
- Node.js 20 o superior

## Ejecución

Desde la raíz del repositorio:

```bash
cp .env.example .env
set -a && source .env && set +a && dotnet run --project backend/src/MesaSitec.Api
cd frontend && npm install && npm run dev
```

La API queda disponible en `http://localhost:5080`, Swagger en
`http://localhost:5080/swagger` y el frontend en `http://localhost:5173`.
La base de datos, las migraciones y los datos iniciales se crean automáticamente.

## Credenciales de prueba

Todos los usuarios usan la contraseña `Sitec.2026`.

- `admin@norte.test` — Admin
- `agente1@norte.test` — Agente
- `agente2@norte.test` — Agente
- `user1@norte.test` — Solicitante
- `user2@norte.test` — Solicitante
- `admin@sur.test` — Admin
- `user1@sur.test` — Solicitante

## Verificación

```bash
dotnet test backend/MesaSitec.sln
cd frontend && npm run type-check && npm run build
```

## Estado

Se encuentran implementados los nueve endpoints, autenticación JWT, aislamiento
por organización, reglas de permisos, máquina de estados, cálculo de SLA,
semillas reproducibles, manejo uniforme de errores y las vistas requeridas del
frontend.
