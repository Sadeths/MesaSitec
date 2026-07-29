using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Entidades;

public sealed class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }

    public bool Activo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}