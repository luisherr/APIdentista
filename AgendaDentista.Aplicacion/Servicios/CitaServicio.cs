using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Aplicacion.Excepciones;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Mapeos;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;

namespace AgendaDentista.Aplicacion.Servicios;

public class CitaServicio : ICitaServicio
{
    private readonly ICitaRepositorio _citaRepositorio;
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly IDentistaRepositorio _dentistaRepositorio;

    public CitaServicio(
        ICitaRepositorio citaRepositorio,
        IPacienteRepositorio pacienteRepositorio,
        IDentistaRepositorio dentistaRepositorio)
    {
        _citaRepositorio = citaRepositorio;
        _pacienteRepositorio = pacienteRepositorio;
        _dentistaRepositorio = dentistaRepositorio;
    }

    public async Task<CitaDto> CrearCitaAsync(CrearCitaDto dto)
    {
        if (dto.FechaHora <= DateTime.Now)
            throw new ValidacionExcepcion("La fecha de la cita debe ser futura.");

        var paciente = await _pacienteRepositorio.ObtenerPorIdAsync(dto.IdPaciente)
            ?? throw new EntidadNoEncontradaExcepcion("Paciente", dto.IdPaciente);

        var dentista = await _dentistaRepositorio.ObtenerPorIdAsync(dto.IdDentista)
            ?? throw new EntidadNoEncontradaExcepcion("Dentista", dto.IdDentista);

        var cita = new Cita
        {
            IdPaciente = dto.IdPaciente,
            IdDentista = dto.IdDentista,
            FechaHora = dto.FechaHora,
            Tratamiento = dto.Tratamiento,
            Estado = EstadoCita.Pendiente,
            Confirmado = false,
            RecordatorioEnviado = false,
            FechaCreacion = DateTime.UtcNow
        };

        var creada = await _citaRepositorio.AgregarAsync(cita);
        creada.Paciente = paciente;
        creada.Dentista = dentista;
        return creada.ToDto();
    }

    public async Task<CitaDto?> ObtenerCitaPorIdAsync(int idCita)
    {
        var cita = await _citaRepositorio.ObtenerPorIdAsync(idCita);
        return cita?.ToDto();
    }

    public async Task<IEnumerable<CitaDto>> ObtenerAgendaAsync(int idDentista, DateTime? fecha)
    {
        var citas = await _citaRepositorio.ObtenerCitasPorDentistaAsync(idDentista, fecha);
        return citas.Select(c => c.ToDto());
    }

    public async Task<CitaDto> ActualizarEstadoAsync(int idCita, EstadoCita nuevoEstado)
    {
        var cita = await _citaRepositorio.ObtenerPorIdAsync(idCita)
            ?? throw new EntidadNoEncontradaExcepcion("Cita", idCita);

        cita.Estado = nuevoEstado;
        cita.FechaActualizacion = DateTime.UtcNow;

        if (nuevoEstado == EstadoCita.Confirmada)
            cita.Confirmado = true;

        await _citaRepositorio.ActualizarAsync(cita);
        return cita.ToDto();
    }

    public async Task<CitaDto> ConfirmarCitaAsync(int idCita)
    {
        return await ActualizarEstadoAsync(idCita, EstadoCita.Confirmada);
    }

    public async Task<CitaDto> CancelarCitaAsync(int idCita)
    {
        return await ActualizarEstadoAsync(idCita, EstadoCita.Cancelada);
    }
}
