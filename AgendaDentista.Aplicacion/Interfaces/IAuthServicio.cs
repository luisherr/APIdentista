using AgendaDentista.Aplicacion.DTOs.Auth;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface IAuthServicio
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegistroAsync(RegistroDto dto);
}
