using AgendaDentista.Aplicacion.DTOs.Paciente;
using AgendaDentista.Aplicacion.Excepciones;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Mapeos;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Dominio.Utilidades;

namespace AgendaDentista.Aplicacion.Servicios;

public class PacienteServicio : IPacienteServicio
{
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly IDentistaRepositorio _dentistaRepositorio;

    public PacienteServicio(IPacienteRepositorio pacienteRepositorio, IDentistaRepositorio dentistaRepositorio)
    {
        _pacienteRepositorio = pacienteRepositorio;
        _dentistaRepositorio = dentistaRepositorio;
    }

    public async Task<PacienteDto> CrearPacienteAsync(CrearPacienteDto dto)
    {
        var dentista = await _dentistaRepositorio.ObtenerPorIdAsync(dto.IdDentista)
            ?? throw new EntidadNoEncontradaExcepcion("Dentista", dto.IdDentista);

        var paciente = new Paciente
        {
            IdDentista = dto.IdDentista,
            Nombre = dto.Nombre,
            Telefono = NormalizadorTelefono.Normalizar(dto.Telefono),
            Email = dto.Email,
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        var creado = await _pacienteRepositorio.AgregarAsync(paciente);
        return creado.ToDto();
    }

    public async Task<PacienteDto?> ObtenerPorIdAsync(int idPaciente)
    {
        var paciente = await _pacienteRepositorio.ObtenerPorIdAsync(idPaciente);
        return paciente?.ToDto();
    }

    public async Task<IEnumerable<PacienteDto>> ObtenerPorDentistaAsync(int idDentista)
    {
        var pacientes = await _pacienteRepositorio.ObtenerPorDentistaAsync(idDentista);
        return pacientes.Select(p => p.ToDto());
    }

    public async Task<PacienteDto?> ObtenerPorTelefonoAsync(string telefono)
    {
        var telefonoNormalizado = NormalizadorTelefono.Normalizar(telefono);
        var paciente = await _pacienteRepositorio.ObtenerPorTelefonoAsync(telefonoNormalizado);
        return paciente?.ToDto();
    }

    public async Task EliminarPacienteAsync(int idPaciente)
    {
        var paciente = await _pacienteRepositorio.ObtenerPorIdAsync(idPaciente)
            ?? throw new EntidadNoEncontradaExcepcion("Paciente", idPaciente);

        await _pacienteRepositorio.EliminarAsync(paciente);
    }
}
