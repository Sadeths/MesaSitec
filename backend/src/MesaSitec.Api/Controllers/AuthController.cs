using MesaSitec.Api.Errores;
using MesaSitec.Aplicacion.Autenticacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ILoginServicio _loginServicio;

    public AuthController(
        ILoginServicio loginServicio)
    {
        _loginServicio = loginServicio;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var errores = Validar(solicitud);

        if (errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status422UnprocessableEntity,
                type: "https://mesasitec.local/errores/validacion",
                title: "Error de validación",
                detail: "Uno o más campos contienen errores.",
                codigo: "VALIDACION",
                errores: errores);
        }

        LoginRespuesta? respuesta =
            await _loginServicio.AutenticarAsync(
                solicitud,
                cancellationToken);

        if (respuesta is null)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status401Unauthorized,
                type: "https://mesasitec.local/errores/no-autenticado",
                title: "No autenticado",
                detail: "El correo o la contraseña son incorrectos.",
                codigo: "NO_AUTENTICADO");
        }

        return Ok(respuesta);
    }

    private static Dictionary<string, string[]> Validar(
        LoginSolicitud solicitud)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(solicitud.Email))
        {
            errores["email"] =
            [
                "El correo electrónico es obligatorio."
            ];
        }

        if (string.IsNullOrWhiteSpace(solicitud.Password))
        {
            errores["password"] =
            [
                "La contraseña es obligatoria."
            ];
        }

        return errores;
    }
}