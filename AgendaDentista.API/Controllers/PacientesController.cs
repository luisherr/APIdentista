using AgendaDentista.Aplicacion.DTOs.Paciente;
using AgendaDentista.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PacientesController : ControllerBase
{
    private readonly IPacienteServicio _pacienteServicio;

    public PacientesController(IPacienteServicio pacienteServicio)
    {
        _pacienteServicio = pacienteServicio;
    }

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Crear([FromBody] CrearPacienteDto dto)
    {
        var paciente = await _pacienteServicio.CrearPacienteAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = paciente.IdPaciente }, paciente);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacienteDto>>> ObtenerPorDentista([FromQuery] int idDentista)
    {
        var pacientes = await _pacienteServicio.ObtenerPorDentistaAsync(idDentista);
        return Ok(pacientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PacienteDto>> ObtenerPorId(int id)
    {
        var paciente = await _pacienteServicio.ObtenerPorIdAsync(id);
        if (paciente == null) return NotFound();
        return Ok(paciente);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        await _pacienteServicio.EliminarPacienteAsync(id);
        return NoContent();
    }
}
