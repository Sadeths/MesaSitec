using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MesaSitec.Infraestructura.Persistencia.Configuraciones;

public sealed class SolicitudConfiguracion
    : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("Solicitudes");

        builder.HasKey(solicitud => solicitud.Id);

        builder.Property(solicitud => solicitud.Id);

        builder.Property(solicitud => solicitud.Codigo)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(solicitud => solicitud.Titulo)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(solicitud => solicitud.Descripcion)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(solicitud => solicitud.Prioridad)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(solicitud => solicitud.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(solictud => solictud.MotivoResolucion)
            .HasMaxLength(4000);

        builder.Property(solicitud => solicitud.MotivoCancelacion)
            .HasMaxLength(4000);

        builder.HasIndex(solicitud => new
        {
            solicitud.TenantId,
            solicitud.Codigo
        }).IsUnique();

        builder.HasIndex(solicitud => new
        {
            solicitud.TenantId,
            solicitud.Estado
        });

        builder.HasIndex(solicitud => solicitud.FechaCreacion);

        builder.HasOne(solicitud => solicitud.Tenant)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Categoria)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Solicitante)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Agente)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}