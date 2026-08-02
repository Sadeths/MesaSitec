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

    private readonly ISolicitudCreacionServicio
        _solicitudCreacionServicio;

    private readonly ISolicitudDetalleServicio
        _solicitudDetalleServicio;

    private readonly ISolicitudEdicionServicio
        _solicitudEdicionServicio;

    public SolicitudesController(
        ISolicitudConsultaServicio solicitudConsultaServicio,
        ISolicitudCreacionServicio solicitudCreacionServicio,
        ISolicitudDetalleServicio solicitudDetalleServicio,
        ISolicitudEdicionServicio solicitudEdicionServicio)
    {
        _solicitudConsultaServicio =
            solicitudConsultaServicio;

        _solicitudCreacionServicio =
            solicitudCreacionServicio;

        _solicitudDetalleServicio =
            solicitudDetalleServicio;

        _solicitudEdicionServicio =
            solicitudEdicionServicio;
    }

    // GET /api/v1/solicitudes
    [HttpGet]
    [ProducesResponseType<SolicitudListadoRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] SolicitudConsulta consulta,
        CancellationToken cancellationToken)
    {
        if (!TryObtenerUsuarioActual(
                out Guid usuarioId,
                out Guid tenantId,
                out RolUsuario rol))
        {
            return CrearErrorNoAutenticado();
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

    // POST /api/v1/solicitudes
    [HttpPost]
    [ProducesResponseType<SolicitudDetalleRespuesta>(
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(
        [FromBody] SolicitudCrearPeticion peticion,
        CancellationToken cancellationToken)
    {
        if (!TryObtenerUsuarioActual(
                out Guid usuarioId,
                out Guid tenantId,
                out _))
        {
            return CrearErrorNoAutenticado();
        }

        Dictionary<string, string[]> errores =
            SolicitudCrearValidador.Validar(peticion);

        if (errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/validacion",
                title: "Error de validación",
                detail:
                    "Uno o más campos contienen errores.",
                codigo: "VALIDACION",
                errores: errores);
        }

        SolicitudCreacionResultado resultado =
            await _solicitudCreacionServicio.CrearAsync(
                tenantId,
                usuarioId,
                peticion,
                cancellationToken);

        if (resultado.NoAutenticado)
        {
            return CrearErrorNoAutenticado();
        }

        if (resultado.Errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/validacion",
                title: "Error de validación",
                detail:
                    "Uno o más campos contienen errores.",
                codigo: "VALIDACION",
                errores: resultado.Errores);
        }

        SolicitudDetalleRespuesta respuesta =
            resultado.Solicitud!;

        return Created(
            $"/api/v1/solicitudes/{respuesta.Id}",
            respuesta);
    }

    // GET /api/v1/solicitudes/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SolicitudDetalleRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryObtenerUsuarioActual(
                out Guid usuarioId,
                out Guid tenantId,
                out RolUsuario rol))
        {
            return CrearErrorNoAutenticado();
        }

        SolicitudDetalleRespuesta? solicitud =
            await _solicitudDetalleServicio.ObtenerAsync(
                id,
                tenantId,
                usuarioId,
                rol,
                cancellationToken);

        if (solicitud is null)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status404NotFound,
                type:
                    "https://mesasitec.local/errores/recurso-no-encontrado",
                title: "Recurso no encontrado",
                detail:
                    "La solicitud indicada no existe.",
                codigo: "RECURSO_NO_ENCONTRADO");
        }

        return Ok(solicitud);
    }

    // PUT /api/v1/solicitudes/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType<SolicitudDetalleRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(
        Guid id,
        [FromBody] SolicitudEditarPeticion peticion,
        CancellationToken cancellationToken)
    {
        if (!TryObtenerUsuarioActual(
                out Guid usuarioId,
                out Guid tenantId,
                out RolUsuario rol))
        {
            return CrearErrorNoAutenticado();
        }

        Dictionary<string, string[]> errores =
            SolicitudEditarValidador.Validar(peticion);

        if (errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/validacion",
                title: "Error de validación",
                detail:
                    "Uno o más campos contienen errores.",
                codigo: "VALIDACION",
                errores: errores);
        }

        SolicitudEdicionResultado resultado =
            await _solicitudEdicionServicio.EditarAsync(
                id,
                tenantId,
                usuarioId,
                rol,
                peticion,
                cancellationToken);

        if (resultado.NoEncontrada)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status404NotFound,
                type:
                    "https://mesasitec.local/errores/recurso-no-encontrado",
                title: "Recurso no encontrado",
                detail:
                    "La solicitud indicada no existe.",
                codigo: "RECURSO_NO_ENCONTRADO");
        }

        if (resultado.OperacionNoPermitida)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status403Forbidden,
                type:
                    "https://mesasitec.local/errores/operacion-no-permitida",
                title: "Operación no permitida",
                detail:
                    "El usuario no tiene permiso para editar esta solicitud.",
                codigo: "OPERACION_NO_PERMITIDA");
        }

        if (resultado.Errores.Count > 0)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/validacion",
                title: "Error de validación",
                detail:
                    "Uno o más campos contienen errores.",
                codigo: "VALIDACION",
                errores: resultado.Errores);
        }

        return Ok(resultado.Solicitud);
    }

    private bool TryObtenerUsuarioActual(
        out Guid usuarioId,
        out Guid tenantId,
        out RolUsuario rol)
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
            out usuarioId);

        bool tenantIdValido = Guid.TryParse(
            tenantIdClaim,
            out tenantId);

        bool rolValido = Enum.TryParse(
            rolClaim,
            ignoreCase: true,
            out rol);

        return usuarioIdValido &&
               tenantIdValido &&
               rolValido &&
               Enum.IsDefined(rol);
    }

    private IActionResult CrearErrorNoAutenticado()
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
}