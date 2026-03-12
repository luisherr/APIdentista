using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class ConversacionWhatsAppConfiguracion : IEntityTypeConfiguration<ConversacionWhatsApp>
{
    public void Configure(EntityTypeBuilder<ConversacionWhatsApp> builder)
    {
        builder.ToTable("ConversacionesWhatsApp");
        builder.HasKey(c => c.IdConversacion);
        builder.Property(c => c.IdConversacion).UseIdentityColumn();
        builder.Property(c => c.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(c => c.HistorialMensajesJson).IsRequired();
        builder.Property(c => c.UltimaActividad).HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.Activa).HasDefaultValue(true);

        builder.HasIndex(c => new { c.Telefono, c.Activa });

        builder.HasOne(c => c.Paciente)
            .WithMany()
            .HasForeignKey(c => c.IdPaciente)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Dentista)
            .WithMany()
            .HasForeignKey(c => c.IdDentista)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
