using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public static class SolicitudCrearValidador
{
    public static Dictionary<string, string[]> Validar(
        SolicitudCrearPeticion peticion)
    {
        var errores = new Dictionary<string, string[]>();

        string titulo = peticion.Titulo?.Trim() ?? string.Empty;

        string descripcion =
            peticion.Descripcion?.Trim() ?? string.Empty;

        if (titulo.Length < 5 || titulo.Length > 120)
        {
            errores["titulo"] =
            [
                "El título debe tener entre 5 y 120 caracteres."
            ];
        }

        if (descripcion.Length < 10 ||
            descripcion.Length > 4000)
        {
            errores["descripcion"] =
            [
                "La descripción debe tener entre 10 y 4000 caracteres."
            ];
        }

        if (peticion.CategoriaId == Guid.Empty)
        {
            errores["categoriaId"] =
            [
                "La categoría es obligatoria."
            ];
        }

        bool prioridadValida =
            Enum.TryParse<PrioridadSolicitud>(
                peticion.Prioridad,
                ignoreCase: true,
                out PrioridadSolicitud prioridad) &&
            Enum.IsDefined(prioridad);

        if (!prioridadValida)
        {
            errores["prioridad"] =
            [
                "La prioridad debe ser Baja, Media, Alta o Critica."
            ];
        }

        return errores;
    }
}