using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Dominio.Interfaces;

public interface IDentistaRepositorio : IRepositorioBase<Dentista>
{
    Task<Dentista?> ObtenerPorEmailAsync(string email);
}
