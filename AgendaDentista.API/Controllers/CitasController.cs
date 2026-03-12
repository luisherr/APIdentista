using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly ICitaServicio _citaServicio;

    public CitasController(ICitaServicio citaServicio)
    {
        _citaServicio = citaServicio;
    }

    [HttpPost]
    public async Task<ActionResult<CitaDto>> Crear([FromBody] CrearCitaDto dto)
    {
        var cita = await _citaServicio.CrearCitaAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cita.IdCita }, cita);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CitaDto>> ObtenerPorId(int id)
    {
        var cita = await _citaServicio.ObtenerCitaPorIdAsync(id);
        if (cita == null) return NotFound();
        return Ok(cita);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CitaDto>>> ObtenerAgenda([FromQuery] int idDentista, [FromQuery] DateTime? fecha)
    {
        var citas = await _citaServicio.ObtenerAgendaAsync(idDentista, fecha);
        return Ok(citas);
    }

    [HttpPut("{id}/confirmar")]
    public async Task<ActionResult<CitaDto>> Confirmar(int id)
    {
        var cita = await _citaServicio.ConfirmarCitaAsync(id);
        return Ok(cita);
    }

    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult<CitaDto>> Cancelar(int id)
    {
        var cita = await _citaServicio.CancelarCitaAsync(id);
        return Ok(cita);
    }

    [HttpPut("{id}/estado")]
    public async Task<ActionResult<CitaDto>> ActualizarEstado(int id, [FromBody] ActualizarEstadoCitaDto dto)
    {
        var cita = await _citaServicio.ActualizarEstadoAsync(id, dto.NuevoEstado);
        return Ok(cita);
    }
}
