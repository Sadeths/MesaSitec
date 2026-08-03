using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class UsuarioConfiguracion
    : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(Usuario => Usuario.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(usuario => usuario.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(usuario => usuario.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(usuario => usuario.Rol)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(usuario => usuario.Activo)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.HasIndex(usuario => usuario.TenantId);

        builder.HasOne(usuario => usuario.Tenant)
            .WithMany()
            .HasForeignKey(usuario => usuario.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}