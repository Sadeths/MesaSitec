namespace MesaSitec.Aplicacion.Autenticacion;

public sealed class LoginSolicitud
{
    public string Email {get; set;} = string.Empty;

    public string Password{get; set;} = string.Empty;
}