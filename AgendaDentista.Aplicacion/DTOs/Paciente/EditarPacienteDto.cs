namespace AgendaDentista.Aplicacion.DTOs.Paciente;

public class EditarPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
}
