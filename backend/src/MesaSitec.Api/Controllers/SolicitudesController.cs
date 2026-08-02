using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MesaSitec.Api.Errores;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public sealed class SolicitudesController : ControllerBase
{
    private readonly ISolicitudConsultaServicio
        _solicitudConsultaServicio;

    public SolicitudesController(
        ISolicitudConsultaServicio solicitudConsultaServicio)
    {
        _solicitudConsultaServicio =
            solicitudConsultaServicio;
    }

    [HttpGet]
    [ProducesResponseType<SolicitudListadoRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] SolicitudConsulta consulta,
        CancellationToken cancellationToken)
    {
        string? usuarioIdClaim =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

        string? tenantIdClaim =
            User.FindFirstValue("tenantId");

        string? rolClaim =
            User.FindFirstValue("rol");

        bool usuarioIdValido = Guid.TryParse(
            usuarioIdClaim,
            out Guid usuarioId);

        bool tenantIdValido = Guid.TryParse(
            tenantIdClaim,
            out Guid tenantId);

        bool rolValido = Enum.TryParse<RolUsuario>(
            rolClaim,
            ignoreCase: true,
            out RolUsuario rol);

        if (!usuarioIdValido ||
            !tenantIdValido ||
            !rolValido)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status401Unauthorized,
                type:
                    "https://mesasitec.local/errores/no-autenticado",
                title: "No autenticado",
                detail:
                    "El token no contiene los datos requeridos.",
                codigo: "NO_AUTENTICADO");
        }

        Dictionary<string, string[]> errores =
            SolicitudConsultaValidador.Validar(consulta);

        if (errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status400BadRequest,
                type:
                    "https://mesasitec.local/errores/parametro-invalido",
                title: "Parámetro inválido",
                detail:
                    "Uno o más parámetros de consulta no son válidos.",
                codigo: "PARAMETRO_INVALIDO",
                errores: errores);
        }

        SolicitudListadoRespuesta respuesta =
            await _solicitudConsultaServicio.ListarAsync(
                tenantId,
                usuarioId,
                rol,
                consulta,
                cancellationToken);

        return Ok(respuesta);
    }
}