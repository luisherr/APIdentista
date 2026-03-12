using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class PacienteConfiguracion : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.IdPaciente);
        builder.Property(p => p.IdPaciente).UseIdentityColumn();
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.FechaRegistro).HasDefaultValueSql("GETDATE()");
        builder.Property(p => p.Activo).HasDefaultValue(true);

        builder.HasIndex(p => p.Telefono);

        builder.HasOne(p => p.Dentista)
            .WithMany(d => d.Pacientes)
            .HasForeignKey(p => p.IdDentista)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
