namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudEditarPeticion
{
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public Guid CategoriaId { get; set; }

    public string Prioridad { get; set; } = string.Empty;
}