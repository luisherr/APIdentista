using AgendaDentista.Aplicacion.DTOs.Dentista;
using AgendaDentista.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DentistasController : ControllerBase
{
    private readonly IDentistaServicio _dentistaServicio;

    public DentistasController(IDentistaServicio dentistaServicio)
    {
        _dentistaServicio = dentistaServicio;
    }

    [HttpPost]
    public async Task<ActionResult<DentistaDto>> Crear([FromBody] CrearDentistaDto dto)
    {
        var dentista = await _dentistaServicio.CrearDentistaAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = dentista.IdDentista }, dentista);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DentistaDto>>> ObtenerTodos()
    {
        var dentistas = await _dentistaServicio.ObtenerTodosAsync();
        return Ok(dentistas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DentistaDto>> ObtenerPorId(int id)
    {
        var dentista = await _dentistaServicio.ObtenerPorIdAsync(id);
        if (dentista == null) return NotFound();
        return Ok(dentista);
    }
}
