using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public static class SolicitudTransicionValidador
{
    public static Dictionary<string, string[]> Validar(
        SolicitudTransicionPeticion peticion)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(peticion.Accion))
        {
            errores["accion"] =
            [
                "La acción es obligatoria."
            ];

            return errores;
        }

        bool accionValida =
            Enum.TryParse<AccionSolicitud>(
                peticion.Accion,
                ignoreCase: true,
                out AccionSolicitud accion) &&
            Enum.IsDefined(
                typeof(AccionSolicitud),
                accion);

        if (!accionValida)
        {
            errores["accion"] =
            [
                "La acción debe ser asignar, iniciar, resolver, cerrar, reabrir o cancelar."
            ];
        }

        return errores;
    }
}