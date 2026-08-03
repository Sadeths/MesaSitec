namespace MesaSitec.Aplicacion.Autenticacion;

public sealed class LoginRespuesta
{
    public string AccessToken { get; set; } = string.Empty;

    public int ExpiraEn { get; set; }

    public UsuarioRespuesta Usuario { get; set; } = null!;
}