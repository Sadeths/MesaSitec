# Decisiones técnicas

## 1. Separación del backend por capas

Dividí el backend en `Api`, `Aplicacion`, `Dominio` e `Infraestructura`. La API se encarga del contrato HTTP, Aplicación contiene los casos de uso, Dominio mantiene las reglas de negocio e Infraestructura administra EF Core, SQLite, autenticación y acceso a datos.

La alternativa era construir un único proyecto y colocar la lógica directamente en los controladores. La descarté porque dificultaría probar de forma aislada la máquina de estados, el cálculo del SLA y los permisos. Tampoco agregué CQRS, MediatR ni repositorios genéricos, porque para el tamaño de esta prueba habrían añadido complejidad innecesaria.

## 2. Aislamiento multiempresa desde el servidor

El `tenantId` se obtiene exclusivamente desde el token JWT y se utiliza para filtrar las consultas. Cuando un recurso pertenece a otra organización, la API devuelve `404 Not Found` para no revelar que dicho recurso existe.

La alternativa era recibir el `tenantId` desde el frontend o como parámetro de cada petición. La descarté porque un usuario podría modificarlo y tratar de acceder a información de otra organización. Preferí que el aislamiento dependiera siempre de la identidad autenticada y no de datos controlados por el cliente.

## 3. SQLite, migraciones y datos semilla automáticos

Elegí SQLite con migraciones automáticas y una semilla determinista basada en `SEED_FECHA_BASE`. Esto permite que el evaluador levante el proyecto sin instalar un servidor de base de datos y que los datos iniciales sean iguales en cada ejecución.

La alternativa era utilizar SQL Server, MySQL o scripts manuales de creación. La descarté porque habría agregado instalaciones y pasos adicionales, aumentando el riesgo de que el proyecto no pudiera iniciarse en menos de cinco minutos.

## Uso de inteligencia artificial

Utilicé ChatGPT y Codex para interpretar el enunciado, proponer estructuras iniciales, generar borradores de algunos servicios, validadores, pruebas y componentes de Vue, revisar el cumplimiento del contrato y preparar parte de la documentación.

No utilicé las respuestas sin revisión. Integré y adapté el código a la estructura real del proyecto, corregí namespaces y referencias, ejecuté migraciones, resolví errores de compilación, probé los endpoints en Swagger, validé los roles y transiciones y ejecuté las pruebas de backend y frontend.

La configuración del repositorio, la integración entre módulos, los ajustes derivados de los errores encontrados, las pruebas manuales y las decisiones finales fueron realizadas y verificadas por mí.

## Qué haría con una semana adicional

Agregaría pruebas de integración para los nueve endpoints y para el aislamiento entre organizaciones. También reemplazaría la lista de agentes semilla utilizada por el frontend con un endpoint específico para consultar usuarios asignables, evitando que el cliente conozca identificadores fijos.

Además, incorporaría Docker Compose, integración continua para ejecutar `dotnet test` y `npm run build`, y mejoraría la accesibilidad y las pruebas automatizadas de la interfaz.

## Punto donde me atasqué

Durante la creación de las pruebas unitarias coloqué accidentalmente los archivos de pruebas dentro de `MesaSitec.Dominio`. Por esta razón el compilador no reconocía `Fact`, `Theory`, `InlineData` ni `Assert`, aunque el código de las pruebas parecía correcto.

Identifiqué el problema revisando la ruta de los archivos y las referencias de cada proyecto. Lo resolví moviendo las pruebas a `MesaSitec.UnitTests`, verificando la referencia al proyecto de Dominio y confirmando que xUnit estuviera instalado. Este error me ayudó a comprender mejor que cada proyecto solo tiene acceso a los paquetes y dependencias declarados en su propio archivo `.csproj`.