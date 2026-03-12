using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class MensajeWhatsAppRepositorio : RepositorioBase<MensajeWhatsApp>, IMensajeWhatsAppRepositorio
{
    public MensajeWhatsAppRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<IEnumerable<MensajeWhatsApp>> ObtenerPorCitaAsync(int idCita)
    {
        return await _dbSet
            .Where(m => m.IdCita == idCita)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<MensajeWhatsApp?> ObtenerPorIdProveedorAsync(string idMensajeProveedor)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.IdMensajeProveedor == idMensajeProveedor);
    }
}
