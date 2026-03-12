namespace AgendaDentista.Aplicacion.DTOs.Paciente;

public class PacienteDto
{
    public int IdPaciente { get; set; }
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}
