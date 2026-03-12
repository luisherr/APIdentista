namespace AgendaDentista.Infraestructura.Configuracion;

public class WhatsAppConfiguracion
{
    public string NumeroTelefonoId { get; set; } = string.Empty;
    public string TokenAcceso { get; set; } = string.Empty;
    public string UrlBase { get; set; } = "https://graph.facebook.com/v18.0";
    public string VerificarToken { get; set; } = string.Empty;
}
