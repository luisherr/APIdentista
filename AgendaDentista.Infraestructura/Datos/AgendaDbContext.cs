using AgendaDentista.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Datos;

public class AgendaDbContext : DbContext
{
    public AgendaDbContext(DbContextOptions<AgendaDbContext> options) : base(options) { }

    public DbSet<Dentista> Dentistas => Set<Dentista>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Recordatorio> Recordatorios => Set<Recordatorio>();
    public DbSet<MensajeWhatsApp> MensajesWhatsApp => Set<MensajeWhatsApp>();
    public DbSet<LogSistema> LogsSistema => Set<LogSistema>();
    public DbSet<ConversacionWhatsApp> ConversacionesWhatsApp => Set<ConversacionWhatsApp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);
    }
}
