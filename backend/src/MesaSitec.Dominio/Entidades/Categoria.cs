namespace MesaSitec.Dominio.Entidades;

public sealed class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int SlaHoras { get; set; }
    public bool Activo { get; set; } = true;
    public Tenant Tenant { get; set; } = null!;

}