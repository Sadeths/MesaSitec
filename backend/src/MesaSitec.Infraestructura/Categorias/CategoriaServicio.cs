using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Categorias;

public sealed class CategoriaServicio : ICategoriaServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public CategoriaServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoriaRespuesta>>
        ListarActivasAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
    {
        List<CategoriaRespuesta> categorias =
            await _dbContext.Categorias
                .AsNoTracking()
                .Where(categoria =>
                    categoria.TenantId == tenantId &&
                    categoria.Activo)
                .OrderBy(categoria => categoria.Nombre)
                .Select(categoria =>
                    new CategoriaRespuesta
                    {
                        Id = categoria.Id,
                        Nombre = categoria.Nombre,
                        SlaHoras = categoria.SlaHoras
                    })
                .ToListAsync(cancellationToken);

        return categorias;
    }
}