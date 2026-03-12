using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Aplicacion.DTOs.Recordatorio;

public class RecordatorioDto
{
    public int IdRecordatorio { get; set; }
    public int IdCita { get; set; }
    public DateTime FechaEnvio { get; set; }
    public TipoRecordatorio TipoRecordatorio { get; set; }
    public EstadoEnvio EstadoEnvio { get; set; }
    public int Intentos { get; set; }
    public string? RespuestaPaciente { get; set; }
}
