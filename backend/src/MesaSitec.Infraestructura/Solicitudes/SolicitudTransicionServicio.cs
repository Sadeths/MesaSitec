using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudTransicionServicio
    : ISolicitudTransicionServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public SolicitudTransicionServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SolicitudTransicionResultado> EjecutarAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudTransicionPeticion peticion,
        CancellationToken cancellationToken)
    {
        Solicitud? solicitud =
            await _dbContext.Solicitudes
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == solicitudId &&
                        item.TenantId == tenantId,
                    cancellationToken);

        if (solicitud is null)
        {
            return new SolicitudTransicionResultado
            {
                NoEncontrada = true
            };
        }

        bool accionValida =
            Enum.TryParse<AccionSolicitud>(
                peticion.Accion,
                ignoreCase: true,
                out AccionSolicitud accion) &&
            Enum.IsDefined(
                typeof(AccionSolicitud),
                accion);

        if (!accionValida)
        {
            return new SolicitudTransicionResultado
            {
                TransicionInvalida = true
            };
        }

        bool esPropietario =
            solicitud.SolicitanteId == usuarioId;

        bool tienePermiso =
            PoliticaPermisosSolicitud
                .PuedeEjecutarTransicion(
                    rol,
                    accion,
                    esPropietario);

        if (!tienePermiso)
        {
            return new SolicitudTransicionResultado
            {
                OperacionNoPermitida = true
            };
        }

        bool transicionValida =
            MaquinaEstadosSolicitud.TryObtenerEstadoDestino(
                solicitud.Estado,
                accion,
                out EstadoSolicitud estadoDestino);

        if (!transicionValida)
        {
            return new SolicitudTransicionResultado
            {
                TransicionInvalida = true
            };
        }

        if (accion == AccionSolicitud.Asignar)
        {
            Usuario? agente =
                await ObtenerAgenteValidoAsync(
                    peticion.AgenteId,
                    tenantId,
                    cancellationToken);

            if (agente is null)
            {
                return new SolicitudTransicionResultado
                {
                    AgenteInvalido = true
                };
            }

            solicitud.AgenteId = agente.Id;
        }

        if (accion == AccionSolicitud.Resolver)
        {
            string motivo =
                peticion.Motivo?.Trim() ?? string.Empty;

            if (motivo.Length < 20)
            {
                return new SolicitudTransicionResultado
                {
                    MotivoRequerido = true
                };
            }

            solicitud.MotivoResolucion = motivo;
            solicitud.FechaResolucion = DateTime.UtcNow;
        }

        if (accion == AccionSolicitud.Cancelar)
        {
            string motivo =
                peticion.Motivo?.Trim() ?? string.Empty;

            if (motivo.Length < 10)
            {
                return new SolicitudTransicionResultado
                {
                    MotivoRequerido = true
                };
            }

            solicitud.MotivoCancelacion = motivo;
        }

        if (accion == AccionSolicitud.Reabrir)
        {
            // La solicitud vuelve a estar pendiente.
            // Como el modelo no tiene historial, limpiamos
            // la información de la resolución anterior.
            solicitud.FechaResolucion = null;
            solicitud.MotivoResolucion = null;
        }

        solicitud.Estado = estadoDestino;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        SolicitudDetalleRespuesta? respuesta =
            await CrearRespuestaAsync(
                solicitud.Id,
                tenantId,
                cancellationToken);

        return new SolicitudTransicionResultado
        {
            Solicitud = respuesta
        };
    }

    private async Task<Usuario?> ObtenerAgenteValidoAsync(
        Guid? agenteId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!agenteId.HasValue ||
            agenteId.Value == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(
                usuario =>
                    usuario.Id == agenteId.Value &&
                    usuario.TenantId == tenantId &&
                    usuario.Activo &&
                    (
                        usuario.Rol == RolUsuario.Agente ||
                        usuario.Rol == RolUsuario.Admin
                    ),
                cancellationToken);
    }

    private async Task<SolicitudDetalleRespuesta?>
        CrearRespuestaAsync(
            Guid solicitudId,
            Guid tenantId,
            CancellationToken cancellationToken)
    {
        DateTime fechaActualUtc = DateTime.UtcNow;

        SolicitudDetalleRespuesta? respuesta =
            await _dbContext.Solicitudes
                .AsNoTracking()
                .Where(item =>
                    item.Id == solicitudId &&
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
                            fechaActualUtc &&

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
            return null;
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