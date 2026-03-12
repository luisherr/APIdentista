using AgendaDentista.Aplicacion.DTOs.Dentista;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface IDentistaServicio
{
    Task<DentistaDto> CrearDentistaAsync(CrearDentistaDto dto);
    Task<DentistaDto?> ObtenerPorIdAsync(int idDentista);
    Task<IEnumerable<DentistaDto>> ObtenerTodosAsync();
}
