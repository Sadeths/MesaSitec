using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudTransicionServicio
{
    Task<SolicitudTransicionResultado> EjecutarAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudTransicionPeticion peticion,
        CancellationToken cancellationToken);
}