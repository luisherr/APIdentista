namespace AgendaDentista.Dominio.Interfaces;

public interface IRepositorioBase<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task<T> AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(T entidad);
}
