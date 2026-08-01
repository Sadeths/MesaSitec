using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Autenticacion;

public sealed class LoginServicio : ILoginServicio
{
    private readonly MesaSitecDbContext _dbContext;
    private readonly IGeneradorJwt _generadorJwt;

    public LoginServicio(
        MesaSitecDbContext dbContext,
        IGeneradorJwt generadorJwt)
    {
        _dbContext = dbContext;
        _generadorJwt = generadorJwt;
    }

    public async Task<LoginRespuesta?> AutenticarAsync(
        LoginSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        string emailNormalizado =
            solicitud.Email.Trim().ToLowerInvariant();

        Usuario? usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Tenant)
            .SingleOrDefaultAsync(
                usuario => usuario.Email == emailNormalizado,
                cancellationToken);

        if (usuario is null ||
            !usuario.Activo ||
            !usuario.Tenant.Activo)
        {
            return null;
        }

        bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(
            solicitud.Password,
            usuario.PasswordHash);

        if (!passwordCorrecto)
        {
            return null;
        }

        var datosToken = new DatosToken
        {
            UsuarioId = usuario.Id,
            TenantId = usuario.TenantId,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString()
        };

        string accessToken =
            _generadorJwt.Generar(datosToken);

        return new LoginRespuesta
        {
            AccessToken = accessToken,
            ExpiraEn = Seguridad.JwtConfiguracion
                .ExpiracionSegundos,

            Usuario = CrearUsuarioRespuesta(usuario)
        };
    }

    private static UsuarioRespuesta CrearUsuarioRespuesta(
        Usuario usuario)
    {
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