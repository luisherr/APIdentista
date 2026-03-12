using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Dominio.Interfaces;

public interface ICitaRepositorio : IRepositorioBase<Cita>
{
    Task<IEnumerable<Cita>> ObtenerCitasProximasAsync(DateTime desde, DateTime hasta);
    Task<IEnumerable<Cita>> ObtenerCitasPendientesRecordatorioAsync();
    Task<IEnumerable<Cita>> ObtenerCitasPorDentistaAsync(int idDentista, DateTime? fecha);
    Task<IEnumerable<Cita>> ObtenerCitasPorPacienteAsync(int idPaciente);
}
