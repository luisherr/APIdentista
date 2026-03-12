using AgendaDentista.Aplicacion.DTOs.Auth;
using AgendaDentista.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgendaDentista.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _authServicio;

    public AuthController(IAuthServicio authServicio)
    {
        _authServicio = authServicio;
    }

    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponseDto>> Registro([FromBody] RegistroDto dto)
    {
        var resultado = await _authServicio.RegistroAsync(dto);
        return Ok(resultado);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authServicio.LoginAsync(dto);
        return Ok(resultado);
    }
}
