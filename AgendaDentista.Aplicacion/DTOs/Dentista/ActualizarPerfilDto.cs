namespace AgendaDentista.Aplicacion.DTOs.Dentista;

public class ActualizarPerfilDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? PasswordActual { get; set; }
    public string? PasswordNuevo { get; set; }
}
