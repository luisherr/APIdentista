using AgendaDentista.Aplicacion.DTOs.WhatsApp;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Infraestructura.Configuracion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/webhook/whatsapp")]
public class WebhookWhatsAppController : ControllerBase
{
    private readonly IWhatsAppServicio _whatsAppServicio;
    private readonly WhatsAppConfiguracion _config;
    private readonly ILogger<WebhookWhatsAppController> _logger;

    public WebhookWhatsAppController(
        IWhatsAppServicio whatsAppServicio,
        IOptions<WhatsAppConfiguracion> config,
        ILogger<WebhookWhatsAppController> logger)
    {
        _whatsAppServicio = whatsAppServicio;
        _config = config.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Verificar(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe" && verifyToken == _config.VerificarToken)
        {
            _logger.LogInformation("Webhook verificado correctamente");
            return Ok(challenge);
        }

        _logger.LogWarning("Intento de verificación de webhook fallido");
        return Forbid();
    }

    [HttpPost]
    public IActionResult RecibirMensaje([FromBody] MensajeEntranteWhatsAppDto mensaje)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _whatsAppServicio.ProcesarMensajeEntranteAsync(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje entrante de WhatsApp");
            }
        });

        return Ok();
    }
}
