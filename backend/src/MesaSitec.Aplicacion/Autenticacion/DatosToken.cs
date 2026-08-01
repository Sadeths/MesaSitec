namespace MesaSitec.Aplicacion.Autenticacion;

public sealed class DatosToken
{
    public Guid UsuarioId {get; set;}

    public Guid TenantId {get; set;}

    public string Email {get; set;} = string.Empty;

    public string Rol {get; set;} = string.Empty;
}