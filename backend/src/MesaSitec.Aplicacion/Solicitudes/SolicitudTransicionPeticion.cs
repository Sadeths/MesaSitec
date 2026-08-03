namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudTransicionPeticion
{
    public string Accion { get; set; } = string.Empty;

    public Guid? AgenteId { get; set; }

    public string? Motivo { get; set; }
}