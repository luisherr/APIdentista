namespace AgendaDentista.Aplicacion.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int IdDentista { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Suscripción / Trial
    public bool SuscripcionActiva { get; set; }
    public string FechaRegistro { get; set; } = string.Empty;
    public bool EnTrial { get; set; }
    public int DiasRestantesTrial { get; set; }
}
