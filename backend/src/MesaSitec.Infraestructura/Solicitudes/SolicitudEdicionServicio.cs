using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudEdicionServicio
    : ISolicitudEdicionServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public SolicitudEdicionServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SolicitudEdicionResultado> EditarAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudEditarPeticion peticion,
        CancellationToken cancellationToken)
    {
        // El tenant se incluye en la consulta para impedir que
        // una organización modifique datos de otra.
        Solicitud? solicitud =
            await _dbContext.Solicitudes
                .SingleOrDefaultAsync(
                    solicitud =>
                        solicitud.Id == solicitudId &&
                        solicitud.TenantId == tenantId,
                    cancellationToken);

        if (solicitud is null)
        {
            return new SolicitudEdicionResultado
            {
                NoEncontrada = true
            };
        }

        // El Solicitante únicamente puede modificar solicitudes
        // propias que todavía estén en estado Nueva.
        if (rol == RolUsuario.Solicitante &&
            (solicitud.SolicitanteId != usuarioId ||
             solicitud.Estado != EstadoSolicitud.Nueva))
        {
            return new SolicitudEdicionResultado
            {
                OperacionNoPermitida = true
            };
        }

        bool prioridadValida =
            Enum.TryParse<PrioridadSolicitud>(
                peticion.Prioridad,
                ignoreCase: true,
                out PrioridadSolicitud prioridad) &&
            Enum.IsDefined(prioridad);

        if (!prioridadValida)
        {
            return new SolicitudEdicionResultado
            {
                Errores = new Dictionary<string, string[]>
                {
                    ["prioridad"] =
                    [
                        "La prioridad indicada no es válida."
                    ]
                }
            };
        }

        Categoria? categoria =
            await _dbContext.Categorias
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    categoria =>
                        categoria.Id == peticion.CategoriaId &&
                        categoria.TenantId == tenantId &&
                        categoria.Activo,
                    cancellationToken);

        if (categoria is null)
        {
            return new SolicitudEdicionResultado
            {
                Errores = new Dictionary<string, string[]>
                {
                    ["categoriaId"] =
                    [
                        "La categoría indicada no existe o no está disponible."
                    ]
                }
            };
        }

        bool cambioCategoria =
            solicitud.CategoriaId != categoria.Id;

        bool cambioPrioridad =
            solicitud.Prioridad != prioridad;

        solicitud.Titulo = peticion.Titulo.Trim();
        solicitud.Descripcion =
            peticion.Descripcion.Trim();

        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = prioridad;

        bool solicitudAunNoResuelta =
            solicitud.Estado is not EstadoSolicitud.Resuelta
                and not EstadoSolicitud.Cerrada
                and not EstadoSolicitud.Cancelada;

        if ((cambioCategoria || cambioPrioridad) &&
            solicitudAunNoResuelta)
        {
            solicitud.FechaLimiteSla =
                CalculadorSla.CalcularFechaLimite(
                    solicitud.FechaCreacion,
                    categoria.SlaHoras,
                    prioridad);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        SolicitudDetalleRespuesta? respuesta =
            await _dbContext.Solicitudes
                .AsNoTracking()
                .Where(item =>
                    item.Id == solicitud.Id &&
                    item.TenantId == tenantId)
                .Select(item =>
                    new SolicitudDetalleRespuesta
                    {
                        Id = item.Id,
                        Codigo = item.Codigo,
                        Titulo = item.Titulo,
                        Descripcion = item.Descripcion,
                        Estado = item.Estado.ToString(),
                        Prioridad = item.Prioridad.ToString(),

                        Categoria =
                            new CategoriaResumenRespuesta
                            {
                                Id = item.Categoria.Id,
                                Nombre = item.Categoria.Nombre
                            },

                        Solicitante =
                            new UsuarioResumenRespuesta
                            {
                                Id = item.Solicitante.Id,
                                Nombre = item.Solicitante.Nombre
                            },

                        Agente = item.AgenteId == null
                            ? null
                            : new UsuarioResumenRespuesta
                            {
                                Id = item.Agente!.Id,
                                Nombre = item.Agente.Nombre
                            },

                        FechaCreacion =
                            item.FechaCreacion,

                        FechaLimiteSla =
                            item.FechaLimiteSla,

                        Vencida =
                            item.FechaLimiteSla <
                            DateTime.UtcNow &&

                            item.Estado !=
                            EstadoSolicitud.Resuelta &&

                            item.Estado !=
                            EstadoSolicitud.Cerrada &&

                            item.Estado !=
                            EstadoSolicitud.Cancelada,

                        FechaResolucion =
                            item.FechaResolucion,

                        MotivoResolucion =
                            item.MotivoResolucion,

                        MotivoCancelacion =
                            item.MotivoCancelacion
                    })
                .SingleOrDefaultAsync(cancellationToken);

        if (respuesta is null)
        {
            return new SolicitudEdicionResultado
            {
                NoEncontrada = true
            };
        }

        respuesta.FechaCreacion =
            AsegurarUtc(respuesta.FechaCreacion);

        respuesta.FechaLimiteSla =
            AsegurarUtc(respuesta.FechaLimiteSla);

        if (respuesta.FechaResolucion.HasValue)
        {
            respuesta.FechaResolucion =
                AsegurarUtc(
                    respuesta.FechaResolucion.Value);
        }

        return new SolicitudEdicionResultado
        {
            Solicitud = respuesta
        };
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