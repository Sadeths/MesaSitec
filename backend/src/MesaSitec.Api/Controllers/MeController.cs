using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MesaSitec.Api.Errores;
using MesaSitec.Aplicacion.Autenticacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController : ControllerBase
{
    private readonly IPerfilServicio _perfilServicio;

    public MeController(
        IPerfilServicio perfilServicio)
    {
        _perfilServicio = perfilServicio;
    }

    [HttpGet]
    [ProducesResponseType<UsuarioRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(
        CancellationToken cancellationToken)
    {
        string? usuarioIdClaim = User.FindFirstValue(
            JwtRegisteredClaimNames.Sub);

        string? tenantIdClaim = User.FindFirstValue(
            "tenantId");

        bool usuarioIdValido = Guid.TryParse(
            usuarioIdClaim,
            out Guid usuarioId);

        bool tenantIdValido = Guid.TryParse(
            tenantIdClaim,
            out Guid tenantId);

        if (!usuarioIdValido || !tenantIdValido)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status401Unauthorized,
                type: "https://mesasitec.local/errores/no-autenticado",
                title: "No autenticado",
                detail: "El token no contiene los datos requeridos.",
                codigo: "NO_AUTENTICADO");
        }

        UsuarioRespuesta? usuario =
            await _perfilServicio.ObtenerAsync(
                usuarioId,
                tenantId,
                cancellationToken);

        if (usuario is null)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status401Unauthorized,
                type: "https://mesasitec.local/errores/no-autenticado",
                title: "No autenticado",
                detail: "El usuario del token ya no está disponible.",
                codigo: "NO_AUTENTICADO");
        }

        return Ok(usuario);
    }
}