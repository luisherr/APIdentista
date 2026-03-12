namespace AgendaDentista.Aplicacion.DTOs.Chatbot;

public class MensajeLlm
{
    public string Rol { get; set; } = string.Empty; // "system", "user", "assistant", "tool"
    public string? Contenido { get; set; }
    public string? NombreHerramienta { get; set; }
    public string? IdLlamadaHerramienta { get; set; }
    public List<LlamadaHerramienta>? LlamadasHerramienta { get; set; }
}

public class LlamadaHerramienta
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ArgumentosJson { get; set; } = "{}";
}

public class LlmRespuesta
{
    public string? Contenido { get; set; }
    public List<LlamadaHerramienta>? LlamadasHerramienta { get; set; }
    public bool RequiereHerramientas => LlamadasHerramienta is { Count: > 0 };
}

public class DefinicionHerramienta
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ParametrosJson { get; set; } = "{}";
}
