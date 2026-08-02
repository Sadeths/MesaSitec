namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudCreacionServicio
{
    Task<SolicitudCreacionResultado> CrearAsync(
        Guid tenantId,
        Guid usuarioId,
        SolicitudCrearPeticion peticion,
        CancellationToken cancellationToken);
}