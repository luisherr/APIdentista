using AgendaDentista.Aplicacion.DTOs.Paciente;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface IPacienteServicio
{
    Task<PacienteDto> CrearPacienteAsync(CrearPacienteDto dto);
    Task<PacienteDto?> ObtenerPorIdAsync(int idPaciente);
    Task<IEnumerable<PacienteDto>> ObtenerPorDentistaAsync(int idDentista);
    Task<PacienteDto?> ObtenerPorTelefonoAsync(string telefono);
    Task EliminarPacienteAsync(int idPaciente);
}
