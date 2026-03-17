namespace AgendaDentista.Aplicacion.DTOs.Cita;

public class EditarCitaDto
{
    public DateTime FechaHora { get; set; }
    public string Tratamiento { get; set; } = string.Empty;
}
