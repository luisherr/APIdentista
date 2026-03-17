using System.Security.Claims;
using AgendaDentista.Aplicacion.DTOs.Suscripcion;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuscripcionesController : ControllerBase
{
    private readonly IDentistaRepositorio _dentistaRepo;
    private readonly IStripeServicio _stripeServicio;
    private readonly ILogger<SuscripcionesController> _logger;

    public SuscripcionesController(
        IDentistaRepositorio dentistaRepo,
        IStripeServicio stripeServicio,
        ILogger<SuscripcionesController> logger)
    {
        _dentistaRepo = dentistaRepo;
        _stripeServicio = stripeServicio;
        _logger = logger;
    }

    [HttpGet("estado")]
    public async Task<ActionResult<SuscripcionEstadoDto>> ObtenerEstado()
    {
        var idDentista = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dentista = await _dentistaRepo.ObtenerPorIdAsync(idDentista);
        if (dentista == null) return NotFound();

        var diasDesdeRegistro = (DateTime.UtcNow - dentista.FechaRegistro).TotalDays;
        var enTrial = diasDesdeRegistro <= 14;
        var diasRestantes = enTrial ? Math.Max(0, (int)Math.Ceiling(14 - diasDesdeRegistro)) : 0;

        return Ok(new SuscripcionEstadoDto
        {
            EnTrial = enTrial,
            DiasRestantesTrial = diasRestantes,
            SuscripcionActiva = dentista.SuscripcionActiva,
            AccesoPermitido = enTrial || dentista.SuscripcionActiva
        });
    }

    [HttpPost("crear-checkout")]
    public async Task<IActionResult> CrearCheckout()
    {
        var idDentista = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dentista = await _dentistaRepo.ObtenerPorIdAsync(idDentista);
        if (dentista == null) return NotFound();

        try
        {
            var url = await _stripeServicio.CrearCheckoutSessionAsync(dentista);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear checkout session para dentista {Id}", idDentista);
            return StatusCode(500, new { error = "Error al procesar el pago" });
        }
    }
}
