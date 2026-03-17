namespace AgendaDentista.Dominio.Entidades;

public class Dentista
{
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }

    // Suscripción / Stripe
    public bool SuscripcionActiva { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime? FechaFinSuscripcion { get; set; }

    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
