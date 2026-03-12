using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class PacienteRepositorio : RepositorioBase<Paciente>, IPacienteRepositorio
{
    public PacienteRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<IEnumerable<Paciente>> ObtenerPorDentistaAsync(int idDentista)
    {
        return await _dbSet
            .Where(p => p.IdDentista == idDentista && p.Activo)
            .ToListAsync();
    }

    public async Task<Paciente?> ObtenerPorTelefonoAsync(string telefono)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Telefono == telefono && p.Activo);
    }
}
