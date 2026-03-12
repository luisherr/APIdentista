using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Dominio.Interfaces;

public interface IMensajeWhatsAppRepositorio : IRepositorioBase<MensajeWhatsApp>
{
    Task<IEnumerable<MensajeWhatsApp>> ObtenerPorCitaAsync(int idCita);
    Task<MensajeWhatsApp?> ObtenerPorIdProveedorAsync(string idMensajeProveedor);
}
