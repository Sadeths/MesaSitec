using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class CategoriaConfiguracion
    : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(categoria => categoria.SlaHoras)
            .IsRequired();

        builder.Property(categoria => categoria.Activo)
            .IsRequired();

        builder.HasIndex(categoria => categoria.TenantId);

        builder.HasOne(categoria => categoria.Tenant)
            .WithMany()
            .HasForeignKey(categoria => categoria.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}