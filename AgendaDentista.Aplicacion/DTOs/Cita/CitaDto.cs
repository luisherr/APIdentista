using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Aplicacion.DTOs.Cita;

public class CitaDto
{
    public int IdCita { get; set; }
    public int IdPaciente { get; set; }
    public int IdDentista { get; set; }
    public string NombrePaciente { get; set; } = string.Empty;
    public string NombreDentista { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Tratamiento { get; set; } = string.Empty;
    public EstadoCita Estado { get; set; }
    public bool Confirmado { get; set; }
    public bool RecordatorioEnviado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
