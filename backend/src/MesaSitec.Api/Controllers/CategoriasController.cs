using System.Security.Claims;
using MesaSitec.Api.Errores;
using MesaSitec.Aplicacion.Categorias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public sealed class CategoriasController : ControllerBase
{
    private readonly ICategoriaServicio _categoriaServicio;

    public CategoriasController(
        ICategoriaServicio categoriaServicio)
    {
        _categoriaServicio = categoriaServicio;
    }

    [HttpGet]
    [ProducesResponseType<
        IReadOnlyList<CategoriaRespuesta>>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        CancellationToken cancellationToken)
    {
        string? tenantIdClaim =
            User.FindFirstValue("tenantId");

        bool tenantIdValido = Guid.TryParse(
            tenantIdClaim,
            out Guid tenantId);

        if (!tenantIdValido)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status401Unauthorized,
                type:
                    "https://mesasitec.local/errores/no-autenticado",
                title: "No autenticado",
                detail:
                    "El token no contiene un tenantId válido.",
                codigo: "NO_AUTENTICADO");
        }

        IReadOnlyList<CategoriaRespuesta> categorias =
            await _categoriaServicio.ListarActivasAsync(
                tenantId,
                cancellationToken);

        return Ok(categorias);
    }
}