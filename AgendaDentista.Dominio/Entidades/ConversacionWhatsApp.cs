namespace AgendaDentista.Dominio.Entidades;

public class ConversacionWhatsApp
{
    public int IdConversacion { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public int? IdPaciente { get; set; }
    public int? IdDentista { get; set; }
    public string HistorialMensajesJson { get; set; } = "[]";
    public DateTime UltimaActividad { get; set; }
    public bool Activa { get; set; }

    public Paciente? Paciente { get; set; }
    public Dentista? Dentista { get; set; }
}
