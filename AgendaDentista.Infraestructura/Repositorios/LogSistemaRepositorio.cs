using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;

namespace AgendaDentista.Infraestructura.Repositorios;

public class LogSistemaRepositorio : ILogSistemaRepositorio
{
    private readonly AgendaDbContext _context;

    public LogSistemaRepositorio(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(TipoLog tipo, string mensaje, string? stackTrace = null)
    {
        var log = new LogSistema
        {
            Tipo = tipo,
            Mensaje = mensaje,
            StackTrace = stackTrace,
            Fecha = DateTime.UtcNow
        };

        await _context.LogsSistema.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
