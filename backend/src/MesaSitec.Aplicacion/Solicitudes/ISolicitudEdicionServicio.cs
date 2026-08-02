using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudEdicionServicio
{
    Task<SolicitudEdicionResultado> EditarAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudEditarPeticion peticion,
        CancellationToken cancellationToken);
}