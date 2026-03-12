namespace AgendaDentista.Aplicacion.DTOs.Cita;

public class CrearCitaDto
{
    public int IdPaciente { get; set; }
    public int IdDentista { get; set; }
    public DateTime FechaHora { get; set; }
    public string Tratamiento { get; set; } = string.Empty;
}
