namespace AgendaDentista.Aplicacion.Configuracion;

public class ChatbotConfiguracion
{
    public OpenAIConfiguracion OpenAI { get; set; } = new();
    public List<PrecioTratamiento> ListaPrecios { get; set; } = new();
    public int SesionExpirarMinutos { get; set; } = 30;
    public int IdDentistaDefault { get; set; } = 1;
    public int MaxMensajesPorHora { get; set; } = 20;
    public int MaxMensajesHistorial { get; set; } = 30;
}

public class OpenAIConfiguracion
{
    public string ApiKey { get; set; } = string.Empty;
    public string Modelo { get; set; } = "gpt-4o-mini";
    public int MaxTokensRespuesta { get; set; } = 300;
}

public class PrecioTratamiento
{
    public string Tratamiento { get; set; } = string.Empty;
    public string PrecioAproximado { get; set; } = string.Empty;
}
