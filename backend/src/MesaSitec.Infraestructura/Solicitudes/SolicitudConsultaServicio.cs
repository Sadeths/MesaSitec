using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudConsultaServicio
    : ISolicitudConsultaServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public SolicitudConsultaServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SolicitudListadoRespuesta> ListarAsync(
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudConsulta consulta,
        CancellationToken cancellationToken)
    {
        DateTime fechaActualUtc = DateTime.UtcNow;

        IQueryable<Solicitud> query =
            _dbContext.Solicitudes
                .AsNoTracking()
                .Where(solicitud =>
                    solicitud.TenantId == tenantId);

        // Los solicitantes solamente pueden listar sus propias solicitudes.
        if (rol == RolUsuario.Solicitante)
        {
            query = query.Where(solicitud =>
                solicitud.SolicitanteId == usuarioId);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Estado))
        {
            EstadoSolicitud estado = Enum.Parse<EstadoSolicitud>(
                consulta.Estado,
                ignoreCase: true);

            query = query.Where(solicitud =>
                solicitud.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Prioridad))
        {
            PrioridadSolicitud prioridad =
                Enum.Parse<PrioridadSolicitud>(
                    consulta.Prioridad,
                    ignoreCase: true);

            query = query.Where(solicitud =>
                solicitud.Prioridad == prioridad);
        }

        if (consulta.CategoriaId.HasValue)
        {
            query = query.Where(solicitud =>
                solicitud.CategoriaId ==
                consulta.CategoriaId.Value);
        }

        if (consulta.AgenteId.HasValue)
        {
            query = query.Where(solicitud =>
                solicitud.AgenteId ==
                consulta.AgenteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Q))
        {
            string texto = consulta.Q.Trim().ToLowerInvariant();
            string patron = $"%{texto}%";

            query = query.Where(solicitud =>
                EF.Functions.Like(
                    solicitud.Titulo.ToLower(),
                    patron) ||

                EF.Functions.Like(
                    solicitud.Descripcion.ToLower(),
                    patron) ||

                EF.Functions.Like(
                    solicitud.Codigo.ToLower(),
                    patron));
        }

        if (consulta.Vencidas == true)
        {
            query = query.Where(solicitud =>
                solicitud.FechaLimiteSla < fechaActualUtc &&

                solicitud.Estado != EstadoSolicitud.Resuelta &&
                solicitud.Estado != EstadoSolicitud.Cerrada &&
                solicitud.Estado != EstadoSolicitud.Cancelada);
        }

        if (consulta.Vencidas == false)
        {
            query = query.Where(solicitud =>
                solicitud.FechaLimiteSla >= fechaActualUtc ||

                solicitud.Estado == EstadoSolicitud.Resuelta ||
                solicitud.Estado == EstadoSolicitud.Cerrada ||
                solicitud.Estado == EstadoSolicitud.Cancelada);
        }

        int total = await query.CountAsync(
            cancellationToken);

        string sort = string.IsNullOrWhiteSpace(consulta.Sort)
            ? "-fechaCreacion"
            : consulta.Sort;

        query = AplicarOrdenamiento(query, sort);

        int registrosAOmitir =
            (consulta.Page - 1) * consulta.PageSize;

        List<SolicitudListadoItemRespuesta> items =
            await query
                .Skip(registrosAOmitir)
                .Take(consulta.PageSize)
                .Select(solicitud =>
                    new SolicitudListadoItemRespuesta
                    {
                        Id = solicitud.Id,
                        Codigo = solicitud.Codigo,
                        Titulo = solicitud.Titulo,
                        Estado = solicitud.Estado.ToString(),
                        Prioridad = solicitud.Prioridad.ToString(),

                        Categoria =
                            new CategoriaResumenRespuesta
                            {
                                Id = solicitud.Categoria.Id,
                                Nombre = solicitud.Categoria.Nombre
                            },

                        Agente = solicitud.AgenteId == null
                            ? null
                            : new UsuarioResumenRespuesta
                            {
                                Id = solicitud.Agente!.Id,
                                Nombre = solicitud.Agente.Nombre
                            },

                        FechaCreacion =
                            solicitud.FechaCreacion,

                        FechaLimiteSla =
                            solicitud.FechaLimiteSla,

                        Vencida =
                            solicitud.FechaLimiteSla <
                            fechaActualUtc &&

                            solicitud.Estado !=
                            EstadoSolicitud.Resuelta &&

                            solicitud.Estado !=
                            EstadoSolicitud.Cerrada &&

                            solicitud.Estado !=
                            EstadoSolicitud.Cancelada
                    })
                .ToListAsync(cancellationToken);

        // SQLite puede recuperar las fechas sin conservar el tipo UTC.
        foreach (SolicitudListadoItemRespuesta item in items)
        {
            item.FechaCreacion =
                AsegurarUtc(item.FechaCreacion);

            item.FechaLimiteSla =
                AsegurarUtc(item.FechaLimiteSla);
        }

        int totalPaginas = total == 0
            ? 0
            : (int)Math.Ceiling(
                total / (double)consulta.PageSize);

        return new SolicitudListadoRespuesta
        {
            Items = items,
            Page = consulta.Page,
            PageSize = consulta.PageSize,
            Total = total,
            TotalPaginas = totalPaginas
        };
    }

    private static IQueryable<Solicitud> AplicarOrdenamiento(
        IQueryable<Solicitud> query,
        string sort)
    {
        return sort switch
        {
            "fechaCreacion" =>
                query
                    .OrderBy(solicitud =>
                        solicitud.FechaCreacion)
                    .ThenBy(solicitud =>
                        solicitud.Codigo),

            "-fechaCreacion" =>
                query
                    .OrderByDescending(solicitud =>
                        solicitud.FechaCreacion)
                    .ThenByDescending(solicitud =>
                        solicitud.Codigo),

            "prioridad" =>
                query
                    .OrderBy(solicitud =>
                        solicitud.Prioridad ==
                        PrioridadSolicitud.Critica ? 4 :

                        solicitud.Prioridad ==
                        PrioridadSolicitud.Alta ? 3 :

                        solicitud.Prioridad ==
                        PrioridadSolicitud.Media ? 2 : 1)
                    .ThenByDescending(solicitud =>
                        solicitud.FechaCreacion),

            "-prioridad" =>
                query
                    .OrderByDescending(solicitud =>
                        solicitud.Prioridad ==
                        PrioridadSolicitud.Critica ? 4 :

                        solicitud.Prioridad ==
                        PrioridadSolicitud.Alta ? 3 :

                        solicitud.Prioridad ==
                        PrioridadSolicitud.Media ? 2 : 1)
                    .ThenByDescending(solicitud =>
                        solicitud.FechaCreacion),

            "codigo" =>
                query.OrderBy(solicitud =>
                    solicitud.Codigo),

            _ =>
                query.OrderByDescending(solicitud =>
                    solicitud.FechaCreacion)
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