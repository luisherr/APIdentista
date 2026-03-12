using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Dominio.Interfaces;

public interface IConversacionRepositorio : IRepositorioBase<ConversacionWhatsApp>
{
    Task<ConversacionWhatsApp?> ObtenerActivaPorTelefonoAsync(string telefono);
}
