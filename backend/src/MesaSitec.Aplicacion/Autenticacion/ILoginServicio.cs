namespace MesaSitec.Aplicacion.Autenticacion;

public interface ILoginServicio
{
    Task<LoginRespuesta?> AutenticarAsync(
        LoginSolicitud solicitud,
        CancellationToken cancellationToken);

}