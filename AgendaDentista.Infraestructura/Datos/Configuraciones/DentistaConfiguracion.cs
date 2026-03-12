using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaDentista.Infraestructura.Datos.Configuraciones;

public class DentistaConfiguracion : IEntityTypeConfiguration<Dentista>
{
    public void Configure(EntityTypeBuilder<Dentista> builder)
    {
        builder.ToTable("Dentistas");
        builder.HasKey(d => d.IdDentista);
        builder.Property(d => d.IdDentista).UseIdentityColumn();
        builder.Property(d => d.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Telefono).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Email).HasMaxLength(200);
        builder.Property(d => d.FechaRegistro).HasDefaultValueSql("GETDATE()");
        builder.Property(d => d.Activo).HasDefaultValue(true);
    }
}
