using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace AgendaDentista.Infraestructura.Repositorios;

public class ConversacionRepositorio : RepositorioBase<ConversacionWhatsApp>, IConversacionRepositorio
{
    public ConversacionRepositorio(AgendaDbContext context) : base(context) { }

    public async Task<ConversacionWhatsApp?> ObtenerActivaPorTelefonoAsync(string telefono)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Telefono == telefono && c.Activa);
    }
}
