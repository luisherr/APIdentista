using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Dominio.Interfaces;

public interface IPacienteRepositorio : IRepositorioBase<Paciente>
{
    Task<IEnumerable<Paciente>> ObtenerPorDentistaAsync(int idDentista);
    Task<Paciente?> ObtenerPorTelefonoAsync(string telefono);
}
