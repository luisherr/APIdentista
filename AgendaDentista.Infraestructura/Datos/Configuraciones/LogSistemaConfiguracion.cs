using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class LogSistemaConfiguracion : IEntityTypeConfiguration<LogSistema>
{
    public void Configure(EntityTypeBuilder<LogSistema> builder)
    {
        builder.ToTable("LogsSistema");
        builder.HasKey(l => l.IdLog);
        builder.Property(l => l.IdLog).UseIdentityColumn();
        builder.Property(l => l.Tipo).HasConversion<int>();
        builder.Property(l => l.Mensaje).IsRequired();
        builder.Property(l => l.Fecha).HasDefaultValueSql("GETDATE()");
    }
}
