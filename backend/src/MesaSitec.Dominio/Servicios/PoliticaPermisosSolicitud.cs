using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Servicios;

public static class PoliticaPermisosSolicitud
{
    public static bool PuedeEjecutarTransicion(
        RolUsuario rol,
        AccionSolicitud accion,
        bool esPropietario)
    {
        if (rol == RolUsuario.Admin)
        {
            return true;
        }

        if (rol == RolUsuario.Agente)
        {
            return accion is
                AccionSolicitud.Asignar or
                AccionSolicitud.Iniciar or
                AccionSolicitud.Resolver or
                AccionSolicitud.Cerrar or
                AccionSolicitud.Reabrir;
        }

        if (rol == RolUsuario.Solicitante)
        {
            return accion == AccionSolicitud.Cerrar &&
                   esPropietario;
        }

        return false;
    }
}