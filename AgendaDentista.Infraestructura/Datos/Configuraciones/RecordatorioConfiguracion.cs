using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class RecordatorioConfiguracion : IEntityTypeConfiguration<Recordatorio>
{
    public void Configure(EntityTypeBuilder<Recordatorio> builder)
    {
        builder.ToTable("Recordatorios");
        builder.HasKey(r => r.IdRecordatorio);
        builder.Property(r => r.IdRecordatorio).UseIdentityColumn();
        builder.Property(r => r.TipoRecordatorio).HasConversion<int>();
        builder.Property(r => r.EstadoEnvio).HasConversion<int>();
        builder.Property(r => r.Intentos).HasDefaultValue(0);
        builder.Property(r => r.RespuestaPaciente).HasMaxLength(500);

        builder.HasIndex(r => new { r.IdCita, r.TipoRecordatorio }).IsUnique();

        builder.HasOne(r => r.Cita)
            .WithMany(c => c.Recordatorios)
            .HasForeignKey(r => r.IdCita)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
