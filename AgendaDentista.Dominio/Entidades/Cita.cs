using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Entidades;

public class Cita
{
    public int IdCita { get; set; }
    public int IdPaciente { get; set; }
    public int IdDentista { get; set; }
    public DateTime FechaHora { get; set; }
    public string Tratamiento { get; set; } = string.Empty;
    public EstadoCita Estado { get; set; }
    public bool Confirmado { get; set; }
    public bool RecordatorioEnviado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    public Paciente Paciente { get; set; } = null!;
    public Dentista Dentista { get; set; } = null!;
    public ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();
}
