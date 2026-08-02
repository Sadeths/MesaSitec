namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudListadoRespuesta
{
    public IReadOnlyList<SolicitudListadoItemRespuesta> Items {get; set;} =
        Array.Empty<SolicitudListadoItemRespuesta>();
    
    public int Page {get; set;}

    public int PageSize {get; set;}

    public int Total {get; set;}

    public int TotalPaginas {get; set;}
}