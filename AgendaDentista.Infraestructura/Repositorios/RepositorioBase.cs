using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class RepositorioBase<T> : IRepositorioBase<T> where T : class
{
    protected readonly AgendaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositorioBase(AgendaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> ObtenerPorIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> ObtenerTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
        await _context.SaveChangesAsync();
        return entidad;
    }

    public async Task ActualizarAsync(T entidad)
    {
        _dbSet.Update(entidad);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(T entidad)
    {
        _dbSet.Remove(entidad);
        await _context.SaveChangesAsync();
    }
}
