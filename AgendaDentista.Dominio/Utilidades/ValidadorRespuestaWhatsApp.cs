using AgendaDentista.Dominio.Enums;

namespace AgendaDentista.Dominio.Utilidades;

public static class ValidadorRespuestaWhatsApp
{
    private static readonly HashSet<string> RespuestasConfirmar = new(StringComparer.OrdinalIgnoreCase)
    {
        "1", "confirmar", "si", "sí", "yes"
    };

    private static readonly HashSet<string> RespuestasCancelar = new(StringComparer.OrdinalIgnoreCase)
    {
        "2", "cancelar", "cancel"
    };

    public static EstadoCita? Validar(string respuesta)
    {
        var texto = respuesta.Trim();

        if (RespuestasConfirmar.Contains(texto))
            return EstadoCita.Confirmada;

        if (RespuestasCancelar.Contains(texto))
            return EstadoCita.Cancelada;

        return null;
    }
}
