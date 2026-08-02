using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudDetalleServicio
    : ISolicitudDetalleServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public SolicitudDetalleServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SolicitudDetalleRespuesta?> ObtenerAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken)
    {
        DateTime fechaActualUtc = DateTime.UtcNow;

        IQueryable<Solicitud> query =
            _dbContext.Solicitudes
                .AsNoTracking()
                .Where(solicitud =>
                    solicitud.Id == solicitudId &&
                    solicitud.TenantId == tenantId);

        // Un solicitante únicamente puede consultar solicitudes propias.
        if (rol == RolUsuario.Solicitante)
        {
            query = query.Where(solicitud =>
                solicitud.SolicitanteId == usuarioId);
        }

        SolicitudDetalleRespuesta? respuesta =
            await query
                .Select(solicitud =>
                    new SolicitudDetalleRespuesta
                    {
                        Id = solicitud.Id,
                        Codigo = solicitud.Codigo,
                        Titulo = solicitud.Titulo,
                        Descripcion = solicitud.Descripcion,
                        Estado = solicitud.Estado.ToString(),
                        Prioridad = solicitud.Prioridad.ToString(),

                        Categoria =
                            new CategoriaResumenRespuesta
                            {
                                Id = solicitud.Categoria.Id,
                                Nombre = solicitud.Categoria.Nombre
                            },

                        Solicitante =
                            new UsuarioResumenRespuesta
                            {
                                Id = solicitud.Solicitante.Id,
                                Nombre = solicitud.Solicitante.Nombre
                            },

                        Agente = solicitud.AgenteId == null
                            ? null
                            : new UsuarioResumenRespuesta
                            {
                                Id = solicitud.Agente!.Id,
                                Nombre = solicitud.Agente.Nombre
                            },

                        FechaCreacion = solicitud.FechaCreacion,
                        FechaLimiteSla = solicitud.FechaLimiteSla,

                        Vencida =
                            solicitud.FechaLimiteSla <
                            fechaActualUtc &&

                            solicitud.Estado !=
                            EstadoSolicitud.Resuelta &&

                            solicitud.Estado !=
                            EstadoSolicitud.Cerrada &&

                            solicitud.Estado !=
                            EstadoSolicitud.Cancelada,

                        FechaResolucion =
                            solicitud.FechaResolucion,

                        MotivoResolucion =
                            solicitud.MotivoResolucion,

                        MotivoCancelacion =
                            solicitud.MotivoCancelacion
                    })
                .SingleOrDefaultAsync(cancellationToken);

        if (respuesta is null)
        {
            return null;
        }

        respuesta.FechaCreacion =
            AsegurarUtc(respuesta.FechaCreacion);

        respuesta.FechaLimiteSla =
            AsegurarUtc(respuesta.FechaLimiteSla);

        if (respuesta.FechaResolucion.HasValue)
        {
            respuesta.FechaResolucion =
                AsegurarUtc(respuesta.FechaResolucion.Value);
        }

        return respuesta;
    }

    private static DateTime AsegurarUtc(
        DateTime fecha)
    {
        return fecha.Kind == DateTimeKind.Utc
            ? fecha
            : DateTime.SpecifyKind(
                fecha,
                DateTimeKind.Utc);
    }
}