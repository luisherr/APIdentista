using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface ICitaServicio
{
    Task<CitaDto> CrearCitaAsync(CrearCitaDto dto);
    Task<CitaDto?> ObtenerCitaPorIdAsync(int idCita);
    Task<IEnumerable<CitaDto>> ObtenerAgendaAsync(int idDentista, DateTime? fecha);
    Task<CitaDto> ActualizarEstadoAsync(int idCita, EstadoCita nuevoEstado);
    Task<CitaDto> ConfirmarCitaAsync(int idCita);
    Task<CitaDto> CancelarCitaAsync(int idCita);
}
