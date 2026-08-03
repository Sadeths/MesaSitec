namespace MesaSitec.Aplicacion.Autenticacion;

public interface IPerfilServicio
{
    Task<UsuarioRespuesta?> ObtenerAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken);

}