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

    private readonly ISolicitudTransicionServicio
        _solicitudTransicionServicio;

    public SolicitudesController(
        ISolicitudConsultaServicio solicitudConsultaServicio,
        ISolicitudCreacionServicio solicitudCreacionServicio,
        ISolicitudDetalleServicio solicitudDetalleServicio,
        ISolicitudEdicionServicio solicitudEdicionServicio,
        ISolicitudTransicionServicio solicitudTransicionServicio)
    {
        _solicitudConsultaServicio =
            solicitudConsultaServicio;

        _solicitudCreacionServicio =
            solicitudCreacionServicio;

        _solicitudDetalleServicio =
            solicitudDetalleServicio;

        _solicitudEdicionServicio =
            solicitudEdicionServicio;

        _solicitudTransicionServicio =
            solicitudTransicionServicio;
    }

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
            return CrearErrorValidacion(errores);
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
            return CrearErrorValidacion(
                resultado.Errores);
        }

        SolicitudDetalleRespuesta respuesta =
            resultado.Solicitud!;

        return Created(
            $"/api/v1/solicitudes/{respuesta.Id}",
            respuesta);
    }

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
            return CrearErrorNoEncontrado();
        }

        return Ok(solicitud);
    }

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
            return CrearErrorValidacion(errores);
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
            return CrearErrorNoEncontrado();
        }

        if (resultado.OperacionNoPermitida)
        {
            return CrearErrorOperacionNoPermitida(
                "El usuario no tiene permiso para editar esta solicitud.");
        }

        if (resultado.Errores.Count > 0)
        {
            return CrearErrorValidacion(
                resultado.Errores);
        }

        return Ok(resultado.Solicitud);
    }

    [HttpPost("{id:guid}/transiciones")]
    [ProducesResponseType<SolicitudDetalleRespuesta>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> EjecutarTransicion(
        Guid id,
        [FromBody] SolicitudTransicionPeticion peticion,
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
            SolicitudTransicionValidador.Validar(peticion);

        if (errores.Count > 0)
        {
            return CrearErrorValidacion(errores);
        }

        SolicitudTransicionResultado resultado =
            await _solicitudTransicionServicio
                .EjecutarAsync(
                    id,
                    tenantId,
                    usuarioId,
                    rol,
                    peticion,
                    cancellationToken);

        if (resultado.NoEncontrada)
        {
            return CrearErrorNoEncontrado();
        }

        if (resultado.OperacionNoPermitida)
        {
            return CrearErrorOperacionNoPermitida(
                "El usuario no tiene permiso para ejecutar esta acción.");
        }

        if (resultado.TransicionInvalida)
        {
            return ProblemasApi.Crear(
                status: StatusCodes.Status409Conflict,
                type:
                    "https://mesasitec.local/errores/transicion-invalida",
                title: "Transición inválida",
                detail:
                    $"No se puede aplicar '{peticion.Accion}' sobre el estado actual de la solicitud.",
                codigo: "TRANSICION_INVALIDA");
        }

        if (resultado.AgenteInvalido)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/agente-invalido",
                title: "Agente inválido",
                detail:
                    "El agente no existe, está inactivo, pertenece a otra organización o no tiene un rol permitido.",
                codigo: "AGENTE_INVALIDO",
                errores:
                    new Dictionary<string, string[]>
                    {
                        ["agenteId"] =
                        [
                            "Debe indicar un agente o administrador activo de la misma organización."
                        ]
                    });
        }

        if (resultado.MotivoRequerido)
        {
            return ProblemasApi.Crear(
                status:
                    StatusCodes.Status422UnprocessableEntity,
                type:
                    "https://mesasitec.local/errores/motivo-requerido",
                title: "Motivo requerido",
                detail:
                    "La acción requiere un motivo con la longitud mínima establecida.",
                codigo: "MOTIVO_REQUERIDO",
                errores:
                    new Dictionary<string, string[]>
                    {
                        ["motivo"] =
                        [
                            "Resolver requiere al menos 20 caracteres y cancelar requiere al menos 10."
                        ]
                    });
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
               Enum.IsDefined(
                   typeof(RolUsuario),
                   rol);
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

    private IActionResult CrearErrorNoEncontrado()
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

    private IActionResult CrearErrorOperacionNoPermitida(
        string detail)
    {
        return ProblemasApi.Crear(
            status: StatusCodes.Status403Forbidden,
            type:
                "https://mesasitec.local/errores/operacion-no-permitida",
            title: "Operación no permitida",
            detail: detail,
            codigo: "OPERACION_NO_PERMITIDA");
    }

    private IActionResult CrearErrorValidacion(
        Dictionary<string, string[]> errores)
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
}