using System.Text.Json;

namespace MesaSitec.Api.Errores;

public sealed class ManejadorGlobalExcepciones
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorGlobalExcepciones> _logger;

    public ManejadorGlobalExcepciones(
        RequestDelegate siguiente,
        ILogger<ManejadorGlobalExcepciones> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            _logger.LogError(
                excepcion,
                "Ocurrió un error no controlado al procesar la solicitud.");

            if (contexto.Response.HasStarted)
            {
                throw;
            }

            contexto.Response.Clear();
            contexto.Response.StatusCode =
                StatusCodes.Status500InternalServerError;
            contexto.Response.ContentType =
                "application/problem+json";

            var problema = new
            {
                type = "https://mesasitec.local/errores/error-interno",
                title = "Error interno",
                status = StatusCodes.Status500InternalServerError,
                detail = "Ocurrió un error inesperado al procesar la solicitud.",
                codigo = "ERROR_INTERNO"
            };

            await JsonSerializer.SerializeAsync(
                contexto.Response.Body,
                problema,
                cancellationToken: contexto.RequestAborted);
        }
    }
}
