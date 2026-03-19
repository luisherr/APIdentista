using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Aplicacion.Excepciones;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Mapeos;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgendaDentista.Aplicacion.Servicios;

public class CitaServicio : ICitaServicio
{
    private readonly ICitaRepositorio _citaRepositorio;
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly IDentistaRepositorio _dentistaRepositorio;
    private readonly IWhatsAppServicio _whatsAppServicio;
    private readonly ILogger<CitaServicio> _logger;

    public CitaServicio(
        ICitaRepositorio citaRepositorio,
        IPacienteRepositorio pacienteRepositorio,
        IDentistaRepositorio dentistaRepositorio,
        IWhatsAppServicio whatsAppServicio,
        ILogger<CitaServicio> logger)
    {
        _citaRepositorio = citaRepositorio;
        _pacienteRepositorio = pacienteRepositorio;
        _dentistaRepositorio = dentistaRepositorio;
        _whatsAppServicio = whatsAppServicio;
        _logger = logger;
    }

    public async Task<CitaDto> CrearCitaAsync(CrearCitaDto dto)
    {
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

        // Enviar notificación por WhatsApp al paciente
        try
        {
            var fecha = dto.FechaHora.ToString("dd/MM/yyyy");
            var hora = dto.FechaHora.ToString("hh:mm tt");
            var mensaje = $"📅 ¡Hola {paciente.Nombre}! Se ha agendado una cita para ti.\n\n" +
                          $"🦷 Tratamiento: {dto.Tratamiento}\n" +
                          $"📆 Fecha: {fecha}\n" +
                          $"🕐 Hora: {hora}\n" +
                          $"👨‍⚕️ Dentista: {dentista.Nombre}\n\n" +
                          "Responde:\n" +
                          "1 - Confirmar cita\n" +
                          "2 - Cancelar cita";

            await _whatsAppServicio.EnviarMensajeAsync(paciente.Telefono, mensaje);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar notificación WhatsApp para cita {IdCita}", creada.IdCita);
        }

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

    public async Task<CitaDto> EditarCitaAsync(int idCita, EditarCitaDto dto)
    {
        var cita = await _citaRepositorio.ObtenerPorIdAsync(idCita)
            ?? throw new EntidadNoEncontradaExcepcion("Cita", idCita);

        if (cita.Estado == EstadoCita.Cancelada)
            throw new ValidacionExcepcion("No se puede editar una cita cancelada.");

        cita.FechaHora = dto.FechaHora;
        cita.Tratamiento = dto.Tratamiento;
        cita.FechaActualizacion = DateTime.UtcNow;

        await _citaRepositorio.ActualizarAsync(cita);
        return cita.ToDto();
    }
}
