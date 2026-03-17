namespace AgendaDentista.Aplicacion.DTOs.Suscripcion;

public class SuscripcionEstadoDto
{
    public bool EnTrial { get; set; }
    public int DiasRestantesTrial { get; set; }
    public bool SuscripcionActiva { get; set; }
    public bool AccesoPermitido { get; set; }
}
