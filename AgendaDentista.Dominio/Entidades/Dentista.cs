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

    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
