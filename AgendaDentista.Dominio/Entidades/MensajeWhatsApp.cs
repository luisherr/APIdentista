using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Entidades;

public class MensajeWhatsApp
{
    public int IdMensaje { get; set; }
    public int IdDentista { get; set; }
    public int IdPaciente { get; set; }
    public int? IdCita { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public TipoMensaje TipoMensaje { get; set; }
    public DateTime Fecha { get; set; }
    public EstadoEnvio EstadoEnvio { get; set; }
    public string? IdMensajeProveedor { get; set; }
}
