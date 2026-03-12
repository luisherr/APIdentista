namespace AgendaDentista.Aplicacion.DTOs.Paciente;

public class CrearPacienteDto
{
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
}
