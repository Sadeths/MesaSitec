using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes;

public static class SolicitudConsultaValidador
{
    private static readonly HashSet<string> OrdenamientosPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "fechaCreacion",
            "-fechaCreacion",
            "prioridad",
            "-prioridad",
            "codigo"
        };

    public static Dictionary<string, string[]> Validar(
        SolicitudConsulta consulta)
    {
        var errores = new Dictionary<string, string[]>();

        if (consulta.Page < 1)
        {
            errores["page"] =
            [
                "El número de página debe ser mayor o igual que 1."
            ];
        }

        if (consulta.PageSize < 1 || consulta.PageSize > 100)
        {
            errores["pageSize"] =
            [
                "El tamaño de página debe estar entre 1 y 100."
            ];
        }

        if (!string.IsNullOrWhiteSpace(consulta.Estado) &&
            !Enum.TryParse<EstadoSolicitud>(
                consulta.Estado,
                ignoreCase: true,
                out _))
        {
            errores["estado"] =
            [
                "El estado indicado no es válido."
            ];
        }

        if (!string.IsNullOrWhiteSpace(consulta.Prioridad) &&
            !Enum.TryParse<PrioridadSolicitud>(
                consulta.Prioridad,
                ignoreCase: true,
                out _))
        {
            errores["prioridad"] =
            [
                "La prioridad indicada no es válida."
            ];
        }

        string sort = string.IsNullOrWhiteSpace(consulta.Sort)
            ? "-fechaCreacion"
            : consulta.Sort;

        if (!OrdenamientosPermitidos.Contains(sort))
        {
            errores["sort"] =
            [
                "El criterio de ordenamiento no es válido."
            ];
        }

        return errores;
    }
}