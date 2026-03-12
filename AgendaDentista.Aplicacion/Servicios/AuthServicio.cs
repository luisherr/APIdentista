using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgendaDentista.Aplicacion.DTOs.Auth;
using AgendaDentista.Aplicacion.Excepciones;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Dominio.Utilidades;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AgendaDentista.Aplicacion.Servicios;

public class AuthServicio : IAuthServicio
{
    private readonly IDentistaRepositorio _dentistaRepositorio;
    private readonly IConfiguration _configuration;

    public AuthServicio(IDentistaRepositorio dentistaRepositorio, IConfiguration configuration)
    {
        _dentistaRepositorio = dentistaRepositorio;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegistroAsync(RegistroDto dto)
    {
        var existente = await _dentistaRepositorio.ObtenerPorEmailAsync(dto.Email);
        if (existente != null)
            throw new ValidacionExcepcion("Ya existe un dentista registrado con ese email.");

        var dentista = new Dentista
        {
            Nombre = dto.Nombre,
            Telefono = NormalizadorTelefono.Normalizar(dto.Telefono),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        await _dentistaRepositorio.AgregarAsync(dentista);

        return new AuthResponseDto
        {
            Token = GenerarToken(dentista),
            IdDentista = dentista.IdDentista,
            Nombre = dentista.Nombre,
            Email = dentista.Email
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var dentista = await _dentistaRepositorio.ObtenerPorEmailAsync(dto.Email);

        if (dentista == null || !BCrypt.Net.BCrypt.Verify(dto.Password, dentista.PasswordHash))
            throw new ValidacionExcepcion("Credenciales inválidas.");

        if (!dentista.Activo)
            throw new ValidacionExcepcion("La cuenta está desactivada.");

        return new AuthResponseDto
        {
            Token = GenerarToken(dentista),
            IdDentista = dentista.IdDentista,
            Nombre = dentista.Nombre,
            Email = dentista.Email
        };
    }

    private string GenerarToken(Dentista dentista)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, dentista.IdDentista.ToString()),
            new Claim(ClaimTypes.Email, dentista.Email),
            new Claim(ClaimTypes.Name, dentista.Nombre)
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                int.Parse(_configuration["Jwt:ExpireHours"] ?? "24")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
