using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class CitaConfiguracion : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("Citas");
        builder.HasKey(c => c.IdCita);
        builder.Property(c => c.IdCita).UseIdentityColumn();
        builder.Property(c => c.Tratamiento).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Estado).HasConversion<int>();
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(c => new { c.IdDentista, c.FechaHora });

        builder.HasOne(c => c.Paciente)
            .WithMany(p => p.Citas)
            .HasForeignKey(c => c.IdPaciente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Dentista)
            .WithMany(d => d.Citas)
            .HasForeignKey(c => c.IdDentista)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
