using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Errores;

public static class ProblemasApi
{
    public static ObjectResult Crear(
        int status,
        string type,
        string title,
        string detail,
        string codigo,
        IDictionary<string, string[]>? errores = null)
    {
        var problema = new ProblemDetails
        {
            Status = status,
            Type = type,
            Title = title,
            Detail = detail
        };

        problema.Extensions["codigo"] = codigo;

        if (errores is not null)
        {
            problema.Extensions["errores"] = errores;
        }

        var resultado = new ObjectResult(problema)
        {
            StatusCode = status
        };

        resultado.ContentTypes.Add(
            "application/problem+json");

        return resultado;
    }
}