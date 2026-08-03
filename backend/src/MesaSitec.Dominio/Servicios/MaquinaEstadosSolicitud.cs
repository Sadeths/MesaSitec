using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Servicios;

public static class MaquinaEstadosSolicitud
{
    public static bool TryObtenerEstadoDestino(
        EstadoSolicitud estadoActual,
        AccionSolicitud accion,
        out EstadoSolicitud estadoDestino)
    {
        switch (estadoActual, accion)
        {
            case (EstadoSolicitud.Nueva, AccionSolicitud.Asignar):
                estadoDestino = EstadoSolicitud.Asignada;
                return true;

            case (EstadoSolicitud.Nueva, AccionSolicitud.Cancelar):
                estadoDestino = EstadoSolicitud.Cancelada;
                return true;

            case (EstadoSolicitud.Asignada, AccionSolicitud.Iniciar):
                estadoDestino = EstadoSolicitud.EnProceso;
                return true;

            case (EstadoSolicitud.Asignada, AccionSolicitud.Asignar):
                estadoDestino = EstadoSolicitud.Asignada;
                return true;

            case (EstadoSolicitud.Asignada, AccionSolicitud.Cancelar):
                estadoDestino = EstadoSolicitud.Cancelada;
                return true;

            case (EstadoSolicitud.EnProceso, AccionSolicitud.Resolver):
                estadoDestino = EstadoSolicitud.Resuelta;
                return true;

            case (EstadoSolicitud.EnProceso, AccionSolicitud.Asignar):
                estadoDestino = EstadoSolicitud.Asignada;
                return true;

            case (EstadoSolicitud.EnProceso, AccionSolicitud.Cancelar):
                estadoDestino = EstadoSolicitud.Cancelada;
                return true;

            case (EstadoSolicitud.Resuelta, AccionSolicitud.Cerrar):
                estadoDestino = EstadoSolicitud.Cerrada;
                return true;

            case (EstadoSolicitud.Resuelta, AccionSolicitud.Reabrir):
                estadoDestino = EstadoSolicitud.EnProceso;
                return true;

            default:
                estadoDestino = estadoActual;
                return false;
        }
    }
}