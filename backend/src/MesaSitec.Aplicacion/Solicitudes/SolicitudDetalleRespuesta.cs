namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudDetalleRespuesta
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public CategoriaResumenRespuesta Categoria { get; set; } = null!;

    public UsuarioResumenRespuesta Solicitante { get; set; } = null!;

    public UsuarioResumenRespuesta? Agente { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaLimiteSla { get; set; }

    public bool Vencida { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? MotivoResolucion { get; set; }

    public string? MotivoCancelacion { get; set; }
}