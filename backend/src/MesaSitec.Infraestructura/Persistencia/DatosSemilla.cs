using System.Globalization;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Persistencia.Semilla;

public static class DatosSemilla
{
    private const string FechaBasePredeterminada =
        "2026-01-15T08:00:00Z";

    public static async Task SembrarAsync(
        MesaSitecDbContext dbContext,
        string? fechaBaseConfigurada)
    {
        // La semilla solo se ejecuta si la base de datos está vacía.
        if (await dbContext.Tenants.AnyAsync())
        {
            return;
        }

        DateTime fechaBase =
            ObtenerFechaBase(fechaBaseConfigurada);

        var tenantNorte = new Tenant
        {
            Id = Guid.Parse(
                "11111111-1111-1111-1111-111111111111"),
            Nombre = "Cooperativa Norte",
            Activo = true
        };

        var tenantSur = new Tenant
        {
            Id = Guid.Parse(
                "22222222-2222-2222-2222-222222222222"),
            Nombre = "Bufete Sur",
            Activo = true
        };

        string passwordHash =
            BCrypt.Net.BCrypt.HashPassword("Sitec.2026");

        var adminNorte = CrearUsuario(
            "10000000-0000-0000-0001-000000000001",
            tenantNorte.Id,
            "admin@norte.test",
            "Administrador Norte",
            RolUsuario.Admin,
            passwordHash);

        var agente1Norte = CrearUsuario(
            "10000000-0000-0000-0001-000000000002",
            tenantNorte.Id,
            "agente1@norte.test",
            "Agente Uno Norte",
            RolUsuario.Agente,
            passwordHash);

        var agente2Norte = CrearUsuario(
            "10000000-0000-0000-0001-000000000003",
            tenantNorte.Id,
            "agente2@norte.test",
            "Agente Dos Norte",
            RolUsuario.Agente,
            passwordHash);

        var user1Norte = CrearUsuario(
            "10000000-0000-0000-0001-000000000004",
            tenantNorte.Id,
            "user1@norte.test",
            "Usuario Uno Norte",
            RolUsuario.Solicitante,
            passwordHash);

        var user2Norte = CrearUsuario(
            "10000000-0000-0000-0001-000000000005",
            tenantNorte.Id,
            "user2@norte.test",
            "Usuario Dos Norte",
            RolUsuario.Solicitante,
            passwordHash);

        var adminSur = CrearUsuario(
            "10000000-0000-0000-0002-000000000001",
            tenantSur.Id,
            "admin@sur.test",
            "Administrador Sur",
            RolUsuario.Admin,
            passwordHash);

        var user1Sur = CrearUsuario(
            "10000000-0000-0000-0002-000000000002",
            tenantSur.Id,
            "user1@sur.test",
            "Usuario Uno Sur",
            RolUsuario.Solicitante,
            passwordHash);

        List<Categoria> categoriasNorte =
            CrearCategorias(tenantNorte.Id, 1);

        List<Categoria> categoriasSur =
            CrearCategorias(tenantSur.Id, 2);

        Guid[] solicitantesNorte =
        {
            user1Norte.Id,
            user2Norte.Id
        };

        Guid[] agentesNorte =
        {
            agente1Norte.Id,
            agente2Norte.Id
        };

        Guid[] solicitantesSur =
        {
            user1Sur.Id
        };

        Guid[] agentesSur =
        {
            adminSur.Id
        };

        List<Solicitud> solicitudesNorte =
            CrearSolicitudes(
                tenantNumero: 1,
                tenantId: tenantNorte.Id,
                solicitantes: solicitantesNorte,
                agentes: agentesNorte,
                categorias: categoriasNorte,
                cantidad: 25,
                fechaBase: fechaBase);

        List<Solicitud> solicitudesSur =
            CrearSolicitudes(
                tenantNumero: 2,
                tenantId: tenantSur.Id,
                solicitantes: solicitantesSur,
                agentes: agentesSur,
                categorias: categoriasSur,
                cantidad: 8,
                fechaBase: fechaBase);

        dbContext.Tenants.AddRange(
            tenantNorte,
            tenantSur);

        dbContext.Usuarios.AddRange(
            adminNorte,
            agente1Norte,
            agente2Norte,
            user1Norte,
            user2Norte,
            adminSur,
            user1Sur);

        dbContext.Categorias.AddRange(categoriasNorte);
        dbContext.Categorias.AddRange(categoriasSur);

        dbContext.Solicitudes.AddRange(solicitudesNorte);
        dbContext.Solicitudes.AddRange(solicitudesSur);

        await dbContext.SaveChangesAsync();
    }

    private static Usuario CrearUsuario(
        string id,
        Guid tenantId,
        string email,
        string nombre,
        RolUsuario rol,
        string passwordHash)
    {
        return new Usuario
        {
            Id = Guid.Parse(id),
            TenantId = tenantId,
            Email = email,
            Nombre = nombre,
            Rol = rol,
            PasswordHash = passwordHash,
            Activo = true
        };
    }

    private static List<Categoria> CrearCategorias(
        Guid tenantId,
        int tenantNumero)
    {
        return new List<Categoria>
        {
            CrearCategoria(
                tenantNumero,
                1,
                tenantId,
                "Incidente",
                8),

            CrearCategoria(
                tenantNumero,
                2,
                tenantId,
                "Requerimiento",
                40),

            CrearCategoria(
                tenantNumero,
                3,
                tenantId,
                "Consulta",
                24),

            CrearCategoria(
                tenantNumero,
                4,
                tenantId,
                "Falla crítica",
                4)
        };
    }

    private static Categoria CrearCategoria(
        int tenantNumero,
        int correlativo,
        Guid tenantId,
        string nombre,
        int slaHoras)
    {
        string id =
            $"20000000-0000-0000-{tenantNumero:0000}-" +
            $"{correlativo:000000000000}";

        return new Categoria
        {
            Id = Guid.Parse(id),
            TenantId = tenantId,
            Nombre = nombre,
            SlaHoras = slaHoras,
            Activo = true
        };
    }

    private static List<Solicitud> CrearSolicitudes(
        int tenantNumero,
        Guid tenantId,
        IReadOnlyList<Guid> solicitantes,
        IReadOnlyList<Guid> agentes,
        IReadOnlyList<Categoria> categorias,
        int cantidad,
        DateTime fechaBase)
    {
        EstadoSolicitud[] estados =
        {
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada
        };

        PrioridadSolicitud[] prioridades =
        {
            PrioridadSolicitud.Baja,
            PrioridadSolicitud.Media,
            PrioridadSolicitud.Alta,
            PrioridadSolicitud.Critica
        };

        var solicitudes = new List<Solicitud>();

        for (int numero = 1; numero <= cantidad; numero++)
        {
            EstadoSolicitud estado =
                estados[(numero - 1) % estados.Length];

            PrioridadSolicitud prioridad =
                prioridades[(numero - 1) % prioridades.Length];

            Categoria categoria =
                categorias[(numero - 1) % categorias.Count];

            Guid solicitanteId =
                solicitantes[(numero - 1) % solicitantes.Count];

            DateTime fechaCreacion =
                fechaBase.AddHours(-18 * numero);

            DateTime fechaLimite =
                CalculadorSla.CalcularFechaLimite(
                    fechaCreacion,
                    categoria.SlaHoras,
                    prioridad);

            bool requiereAgente =
                estado is EstadoSolicitud.Asignada
                    or EstadoSolicitud.EnProceso
                    or EstadoSolicitud.Resuelta
                    or EstadoSolicitud.Cerrada;

            Guid? agenteId = requiereAgente
                ? agentes[(numero - 1) % agentes.Count]
                : null;

            bool fueResuelta =
                estado is EstadoSolicitud.Resuelta
                    or EstadoSolicitud.Cerrada;

            DateTime? fechaResolucion = fueResuelta
                ? fechaCreacion.AddHours(2 + numero % 4)
                : null;

            string? motivoResolucion = fueResuelta
                ? "La solicitud fue atendida y validada correctamente con el usuario."
                : null;

            string? motivoCancelacion =
                estado == EstadoSolicitud.Cancelada
                    ? "Solicitud cancelada porque fue registrada por duplicado."
                    : null;

            solicitudes.Add(new Solicitud
            {
                Id = CrearIdSolicitud(
                    tenantNumero,
                    numero),

                TenantId = tenantId,

                Codigo =
                    $"SOL-{fechaBase.Year}-{numero:00000}",

                Titulo =
                    $"Solicitud de soporte número {numero}",

                Descripcion =
                    $"Descripción de prueba para la solicitud " +
                    $"{numero} de la organización.",

                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = solicitanteId,
                AgenteId = agenteId,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite,
                FechaResolucion = fechaResolucion,
                MotivoResolucion = motivoResolucion,
                MotivoCancelacion = motivoCancelacion
            });
        }

        return solicitudes;
    }

    private static Guid CrearIdSolicitud(
        int tenantNumero,
        int correlativo)
    {
        string id =
            $"30000000-0000-0000-{tenantNumero:0000}-" +
            $"{correlativo:000000000000}";

        return Guid.Parse(id);
    }

    private static DateTime ObtenerFechaBase(
        string? fechaBaseConfigurada)
    {
        string valor = string.IsNullOrWhiteSpace(
            fechaBaseConfigurada)
            ? FechaBasePredeterminada
            : fechaBaseConfigurada;

        bool fechaValida = DateTimeOffset.TryParse(
            valor,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal |
            DateTimeStyles.AdjustToUniversal,
            out DateTimeOffset fecha);

        if (!fechaValida)
        {
            throw new InvalidOperationException(
                $"SEED_FECHA_BASE no contiene una fecha válida: " +
                $"'{valor}'.");
        }

        return fecha.UtcDateTime;
    }
}