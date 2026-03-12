using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Entidades;

public class Recordatorio
{
    public int IdRecordatorio { get; set; }
    public int IdCita { get; set; }
    public DateTime FechaEnvio { get; set; }
    public TipoRecordatorio TipoRecordatorio { get; set; }
    public EstadoEnvio EstadoEnvio { get; set; }
    public int Intentos { get; set; }
    public string? RespuestaPaciente { get; set; }

    public Cita Cita { get; set; } = null!;
}
