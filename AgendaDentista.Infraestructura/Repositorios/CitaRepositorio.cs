using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class CitaRepositorio : RepositorioBase<Cita>, ICitaRepositorio
{
    public CitaRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<IEnumerable<Cita>> ObtenerCitasProximasAsync(DateTime desde, DateTime hasta)
    {
        return await _dbSet
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Where(c => c.FechaHora >= desde && c.FechaHora <= hasta)
            .OrderBy(c => c.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> ObtenerCitasPendientesRecordatorioAsync()
    {
        var ahora = DateTime.UtcNow;
        var limite = ahora.AddHours(24);

        return await _dbSet
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Where(c => c.Estado == EstadoCita.Pendiente
                && !c.RecordatorioEnviado
                && c.FechaHora >= ahora
                && c.FechaHora <= limite)
            .OrderBy(c => c.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> ObtenerCitasPorDentistaAsync(int idDentista, DateTime? fecha)
    {
        var query = _dbSet
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Where(c => c.IdDentista == idDentista);

        if (fecha.HasValue)
        {
            var inicio = fecha.Value.Date;
            var fin = inicio.AddDays(1);
            query = query.Where(c => c.FechaHora >= inicio && c.FechaHora < fin);
        }

        return await query.OrderBy(c => c.FechaHora).ToListAsync();
    }

    public async Task<IEnumerable<Cita>> ObtenerCitasPorPacienteAsync(int idPaciente)
    {
        return await _dbSet
            .Include(c => c.Paciente)
            .Include(c => c.Dentista)
            .Where(c => c.IdPaciente == idPaciente)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();
    }
}
