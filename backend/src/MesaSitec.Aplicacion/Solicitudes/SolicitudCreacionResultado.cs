namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudCreacionResultado
{
    public SolicitudDetalleRespuesta? Solicitud { get; set; }

    public Dictionary<string, string[]> Errores { get; set; } = new();

    public bool NoAutenticado { get; set; }
}