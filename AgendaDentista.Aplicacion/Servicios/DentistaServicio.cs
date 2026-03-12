using AgendaDentista.Aplicacion.DTOs.Dentista;
using AgendaDentista.Aplicacion.Excepciones;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Mapeos;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Dominio.Utilidades;

namespace AgendaDentista.Aplicacion.Servicios;

public class DentistaServicio : IDentistaServicio
{
    private readonly IDentistaRepositorio _dentistaRepositorio;

    public DentistaServicio(IDentistaRepositorio dentistaRepositorio)
    {
        _dentistaRepositorio = dentistaRepositorio;
    }

    public async Task<DentistaDto> CrearDentistaAsync(CrearDentistaDto dto)
    {
        var dentista = new Dentista
        {
            Nombre = dto.Nombre,
            Telefono = NormalizadorTelefono.Normalizar(dto.Telefono),
            Email = dto.Email,
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        var creado = await _dentistaRepositorio.AgregarAsync(dentista);
        return creado.ToDto();
    }

    public async Task<DentistaDto?> ObtenerPorIdAsync(int idDentista)
    {
        var dentista = await _dentistaRepositorio.ObtenerPorIdAsync(idDentista);
        return dentista?.ToDto();
    }

    public async Task<IEnumerable<DentistaDto>> ObtenerTodosAsync()
    {
        var dentistas = await _dentistaRepositorio.ObtenerTodosAsync();
        return dentistas.Select(d => d.ToDto());
    }
}
