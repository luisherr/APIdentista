using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Interfaces;

public interface ILogSistemaRepositorio
{
    Task RegistrarAsync(TipoLog tipo, string mensaje, string? stackTrace = null);
}
