using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudConsultaServicio
{
    Task<SolicitudListadoRespuesta> ListarAsync(
        Guid tenantId,
        Guid usuarioId,
        RolUsuario rol,
        SolicitudConsulta consulta,
        CancellationToken cancellationToken);
}