using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudDetalleServicio
{
    Task<SolicitudDetalleRespuesta?> ObtenerAsync(
        Guid solicitudId,
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        CancellationToken cancellationToken);
}