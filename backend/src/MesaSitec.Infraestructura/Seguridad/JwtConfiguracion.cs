namespace MesaSitec.Infraestructura.Seguridad;

public static class JwtConfiguracion
{
    public const string Emisor = "MesaSitec.Api";

    public const string Audiencia = "MesaSitec.Frotend";

    public const int ExpiracionSegundos = 28800;
}