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

        return errores;
    }
}
