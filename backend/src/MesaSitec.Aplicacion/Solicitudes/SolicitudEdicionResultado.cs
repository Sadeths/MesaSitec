namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudEdicionResultado
{
    public SolicitudDetalleRespuesta? Solicitud { get; set; }

    public Dictionary<string, string[]> Errores { get; set; } =
        new();

    public bool NoEncontrada { get; set; }

    public bool OperacionNoPermitida { get; set; }
}