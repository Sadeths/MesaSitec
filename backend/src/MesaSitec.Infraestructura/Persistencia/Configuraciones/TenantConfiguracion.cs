using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class TenantConfiguracion
    : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(Tenant => Tenant.Id);
        
        builder.Property(tenant => tenant.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(tenant => tenant.Activo)
            .IsRequired();

    }
}