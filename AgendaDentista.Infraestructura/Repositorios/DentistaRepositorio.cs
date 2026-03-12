using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class DentistaRepositorio : RepositorioBase<Dentista>, IDentistaRepositorio
{
    public DentistaRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<Dentista?> ObtenerPorEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Email == email);
    }
}
