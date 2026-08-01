using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Autenticacion;

public sealed class PerfilServicio : IPerfilServicio
{
    private readonly MesaSitecDbContext _dbContext;

    public PerfilServicio(
        MesaSitecDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UsuarioRespuesta?> ObtenerAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        Usuario? usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Tenant)
            .SingleOrDefaultAsync(
                usuario =>
                    usuario.Id == usuarioId &&
                    usuario.TenantId == tenantId &&
                    usuario.Activo &&
                    usuario.Tenant.Activo,
                cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        return new UsuarioRespuesta
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            TenantId = usuario.TenantId,
            TenantNombre = usuario.Tenant.Nombre
        };
    }
}