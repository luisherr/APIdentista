using AgendaDentista.Aplicacion.DTOs.WhatsApp;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface IWhatsAppServicio
{
    Task<bool> EnviarMensajeAsync(string telefono, string mensaje);
    Task ProcesarMensajeEntranteAsync(MensajeEntranteWhatsAppDto mensajeEntrante);
}
