namespace AgendaDentista.Dominio.Entidades;

public class Paciente
{
    public int IdPaciente { get; set; }
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }

    public Dentista Dentista { get; set; } = null!;
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
