# Decisiones técnicas

## Arquitectura

Separé el backend en Dominio, Aplicación, Infraestructura y API. El dominio
contiene las reglas que se pueden probar sin base de datos: máquina de estados,
permisos y SLA. Los controladores se limitan a validar el contrato HTTP y delegar
el trabajo a servicios. Elegí esta separación porque es sencilla de explicar y
evita agregar patrones que no aportan al tamaño de la prueba.

El aislamiento por organización se aplica en cada consulta usando el `tenantId`
del JWT. Los recursos de otro tenant se devuelven como no encontrados. SQLite y
las migraciones automáticas permiten ejecutar el proyecto sin instalar una base
de datos adicional.

## Frontend

Usé Vue 3 con `script setup`, TypeScript estricto, Pinia y Vue Router. Existe un
solo cliente HTTP para agregar el token y manejar respuestas 401. Los filtros y
la paginación siempre consultan al servidor. El formulario de creación se
reutiliza para edición y los botones del detalle combinan las reglas de estado y
rol antes de renderizarse.

El contrato no incluye un endpoint para listar agentes. Por eso la asignación
muestra los agentes conocidos de los datos semilla de la organización actual.
En una versión posterior agregaría un endpoint de agentes, pero no amplié el
contrato obligatorio de la prueba.

## Uso de IA

Utilicé ChatGPT como apoyo para revisar el enunciado, detectar faltantes,
implementar parte del frontend y verificar el contrato. Revisé los cambios y
mantengo la estructura deliberadamente simple para poder explicarla y
modificarla durante la entrevista.
