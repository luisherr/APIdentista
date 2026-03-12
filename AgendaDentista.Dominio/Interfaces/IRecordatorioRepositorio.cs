using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Interfaces;

public interface IRecordatorioRepositorio : IRepositorioBase<Recordatorio>
{
    Task<bool> ExisteRecordatorioParaCitaAsync(int idCita, TipoRecordatorio tipo);
    Task<IEnumerable<Recordatorio>> ObtenerRecordatoriosFallidosAsync();
}
