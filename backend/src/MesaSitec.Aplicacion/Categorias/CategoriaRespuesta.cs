namespace MesaSitec.Aplicacion.Categorias;

public sealed class CategoriaRespuesta
{
    public Guid Id {get; set;}

    public string Nombre {get; set;} = string.Empty;

    public int SlaHoras {get; set;}
}