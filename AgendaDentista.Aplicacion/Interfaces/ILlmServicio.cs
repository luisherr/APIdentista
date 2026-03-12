using AgendaDentista.Aplicacion.DTOs.Chatbot;

namespace AgendaDentista.Aplicacion.Interfaces;

public interface ILlmServicio
{
    Task<LlmRespuesta> ObtenerRespuestaAsync(
        List<MensajeLlm> historial,
        List<DefinicionHerramienta>? herramientas = null);
}
