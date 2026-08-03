namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudTransicionResultado
{
    public SolicitudDetalleRespuesta? Solicitud {get; set;}

    public bool NoEncontrada {get; set;}

    public bool OperacionNoPermitida {get; set;}

    public bool TransicionInvalida {get; set;}

    public bool AgenteInvalido {get; set;}

    public bool MotivoRequerido {get; set;}
}