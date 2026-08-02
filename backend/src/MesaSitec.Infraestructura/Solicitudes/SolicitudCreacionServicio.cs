using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudCreacionServicio
    : ISolicitudCreacionServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public SolicitudCreacionServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SolicitudCreacionResultado> CrearAsync(
        Guid tenantId,
        Guid usuarioId,
        SolicitudCrearPeticion peticion,
        CancellationToken cancellationToken)
    {
        bool prioridadValida =
            Enum.TryParse<PrioridadSolicitud>(
                peticion.Prioridad,
                ignoreCase: true,
                out PrioridadSolicitud prioridad) &&
            Enum.IsDefined(prioridad);

        if (!prioridadValida)
        {
            return new SolicitudCreacionResultado
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

        Categoria? categoria = await _dbContext.Categorias
            .AsNoTracking()
            .SingleOrDefaultAsync(
                categoria =>
                    categoria.Id == peticion.CategoriaId &&
                    categoria.TenantId == tenantId &&
                    categoria.Activo,
                cancellationToken);

        if (categoria is null)
        {
            return new SolicitudCreacionResultado
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

        UsuarioResumenRespuesta? solicitante =
            await _dbContext.Usuarios
                .AsNoTracking()
                .Where(usuario =>
                    usuario.Id == usuarioId &&
                    usuario.TenantId == tenantId &&
                    usuario.Activo)
                .Select(usuario =>
                    new UsuarioResumenRespuesta
                    {
                        Id = usuario.Id,
                        Nombre = usuario.Nombre
                    })
                .SingleOrDefaultAsync(cancellationToken);

        if (solicitante is null)
        {
            return new SolicitudCreacionResultado
            {
                NoAutenticado = true
            };
        }

        DateTime fechaCreacion = DateTime.UtcNow;

        string codigo = await GenerarCodigoAsync(
            tenantId,
            fechaCreacion.Year,
            cancellationToken);

        DateTime fechaLimiteSla =
            CalculadorSla.CalcularFechaLimite(
                fechaCreacion,
                categoria.SlaHoras,
                prioridad);

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo,
            Titulo = peticion.Titulo.Trim(),
            Descripcion = peticion.Descripcion.Trim(),
            CategoriaId = categoria.Id,
            Prioridad = prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = usuarioId,
            AgenteId = null,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaLimiteSla,
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        _dbContext.Solicitudes.Add(solicitud);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var respuesta = new SolicitudDetalleRespuesta
        {
            Id = solicitud.Id,
            Codigo = solicitud.Codigo,
            Titulo = solicitud.Titulo,
            Descripcion = solicitud.Descripcion,
            Estado = solicitud.Estado.ToString(),
            Prioridad = solicitud.Prioridad.ToString(),

            Categoria = new CategoriaResumenRespuesta
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            },

            Solicitante = solicitante,
            Agente = null,
            FechaCreacion = solicitud.FechaCreacion,
            FechaLimiteSla = solicitud.FechaLimiteSla,
            Vencida = false,
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        return new SolicitudCreacionResultado
        {
            Solicitud = respuesta
        };
    }

    private async Task<string> GenerarCodigoAsync(
        Guid tenantId,
        int anio,
        CancellationToken cancellationToken)
    {
        string prefijo = $"SOL-{anio}-";

        string? ultimoCodigo =
            await _dbContext.Solicitudes
                .AsNoTracking()
                .Where(solicitud =>
                    solicitud.TenantId == tenantId &&
                    solicitud.Codigo.StartsWith(prefijo))
                .OrderByDescending(solicitud =>
                    solicitud.Codigo)
                .Select(solicitud =>
                    solicitud.Codigo)
                .FirstOrDefaultAsync(cancellationToken);

        int siguienteCorrelativo = 1;

        if (!string.IsNullOrWhiteSpace(ultimoCodigo) &&
            ultimoCodigo.Length >= 5 &&
            int.TryParse(
                ultimoCodigo[^5..],
                out int ultimoCorrelativo))
        {
            siguienteCorrelativo =
                ultimoCorrelativo + 1;
        }

        return $"{prefijo}{siguienteCorrelativo:00000}";
    }
}