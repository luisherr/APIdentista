using AgendaDentista.Infraestructura.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/webhook")]
[AllowAnonymous]
public class WebhookStripeController : ControllerBase
{
    private readonly IStripeServicio _stripeServicio;
    private readonly ILogger<WebhookStripeController> _logger;

    public WebhookStripeController(IStripeServicio stripeServicio, ILogger<WebhookStripeController> logger)
    {
        _stripeServicio = stripeServicio;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> RecibirEvento()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
            return BadRequest("Falta header Stripe-Signature");

        try
        {
            await _stripeServicio.ProcesarWebhookAsync(json, signature);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando webhook de Stripe");
            return BadRequest(new { error = "Webhook inválido" });
        }
    }
}
