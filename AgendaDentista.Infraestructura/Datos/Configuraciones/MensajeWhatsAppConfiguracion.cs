using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class MensajeWhatsAppConfiguracion : IEntityTypeConfiguration<MensajeWhatsApp>
{
    public void Configure(EntityTypeBuilder<MensajeWhatsApp> builder)
    {
        builder.ToTable("MensajesWhatsApp");
        builder.HasKey(m => m.IdMensaje);
        builder.Property(m => m.IdMensaje).UseIdentityColumn();
        builder.Property(m => m.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Mensaje).IsRequired().HasMaxLength(4000);
        builder.Property(m => m.TipoMensaje).HasConversion<int>();
        builder.Property(m => m.EstadoEnvio).HasConversion<int>();
        builder.Property(m => m.IdMensajeProveedor).HasMaxLength(200);
        builder.Property(m => m.Fecha).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(m => m.IdMensajeProveedor);
    }
}
