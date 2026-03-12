namespace AgendaDentista.Aplicacion.Interfaces;

public interface IChatbotServicio
{
    Task<string> ProcesarMensajeAsync(string telefono, string mensaje, string? nombrePerfil = null);
}
