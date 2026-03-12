using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class RecordatorioRepositorio : RepositorioBase<Recordatorio>, IRecordatorioRepositorio
{
    public RecordatorioRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<bool> ExisteRecordatorioParaCitaAsync(int idCita, TipoRecordatorio tipo)
    {
        return await _dbSet.AnyAsync(r => r.IdCita == idCita
            && r.TipoRecordatorio == tipo
            && r.EstadoEnvio == EstadoEnvio.Enviado);
    }

    public async Task<IEnumerable<Recordatorio>> ObtenerRecordatoriosFallidosAsync()
    {
        return await _dbSet
            .Include(r => r.Cita)
                .ThenInclude(c => c.Paciente)
            .Include(r => r.Cita)
                .ThenInclude(c => c.Dentista)
            .Where(r => r.EstadoEnvio == EstadoEnvio.Fallido && r.Intentos < 3)
            .ToListAsync();
    }
}
