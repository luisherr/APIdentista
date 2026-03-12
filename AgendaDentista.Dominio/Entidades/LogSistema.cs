using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Entidades;

public class LogSistema
{
    public int IdLog { get; set; }
    public TipoLog Tipo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime Fecha { get; set; }
}
