using System.ClientModel;
using System.Text.Json;
using AgendaDentista.Aplicacion.DTOs.Chatbot;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AgendaDentista.Infraestructura.Servicios;

public class OpenAILlmServicio : ILlmServicio
{
    private readonly ChatClient _chatClient;
    private readonly ChatbotConfiguracion _config;
    private readonly ILogger<OpenAILlmServicio> _logger;

    public OpenAILlmServicio(
        IOptions<ChatbotConfiguracion> config,
        ILogger<OpenAILlmServicio> logger)
    {
        _config = config.Value;
        _logger = logger;
        _chatClient = new ChatClient(_config.OpenAI.Modelo, _config.OpenAI.ApiKey);
    }

    public async Task<LlmRespuesta> ObtenerRespuestaAsync(
        List<MensajeLlm> historial,
        List<DefinicionHerramienta>? herramientas = null)
    {
        var mensajes = ConvertirMensajes(historial);
        var opciones = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _config.OpenAI.MaxTokensRespuesta
        };

        if (herramientas != null)
        {
            foreach (var herramienta in herramientas)
            {
                opciones.Tools.Add(ChatTool.CreateFunctionTool(
                    herramienta.Nombre,
                    herramienta.Descripcion,
                    BinaryData.FromString(herramienta.ParametrosJson)));
            }
        }

        try
        {
            ChatCompletion respuesta = await _chatClient.CompleteChatAsync(mensajes, opciones);

            if (respuesta.FinishReason == ChatFinishReason.ToolCalls)
            {
                var llamadas = respuesta.ToolCalls.Select(tc => new LlamadaHerramienta
                {
                    Id = tc.Id,
                    Nombre = tc.FunctionName,
                    ArgumentosJson = tc.FunctionArguments.ToString()
                }).ToList();

                return new LlmRespuesta
                {
                    Contenido = respuesta.Content.Count > 0
                        ? respuesta.Content[0].Text
                        : null,
                    LlamadasHerramienta = llamadas
                };
            }

            return new LlmRespuesta
            {
                Contenido = respuesta.Content.Count > 0
                    ? respuesta.Content[0].Text
                    : "Lo siento, no pude generar una respuesta."
            };
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "Error en llamada a OpenAI API");
            return new LlmRespuesta
            {
                Contenido = "Disculpa, estoy teniendo problemas técnicos. Por favor intenta de nuevo en unos minutos."
            };
        }
    }

    private static List<ChatMessage> ConvertirMensajes(List<MensajeLlm> historial)
    {
        var mensajes = new List<ChatMessage>();

        foreach (var msg in historial)
        {
            switch (msg.Rol)
            {
                case "system":
                    mensajes.Add(ChatMessage.CreateSystemMessage(msg.Contenido ?? ""));
                    break;

                case "user":
                    mensajes.Add(ChatMessage.CreateUserMessage(msg.Contenido ?? ""));
                    break;

                case "assistant":
                    if (msg.LlamadasHerramienta is { Count: > 0 })
                    {
                        var toolCalls = msg.LlamadasHerramienta.Select(lh =>
                            ChatToolCall.CreateFunctionToolCall(
                                lh.Id,
                                lh.Nombre,
                                BinaryData.FromString(lh.ArgumentosJson))).ToList();

                        mensajes.Add(ChatMessage.CreateAssistantMessage(toolCalls));
                    }
                    else
                    {
                        mensajes.Add(ChatMessage.CreateAssistantMessage(msg.Contenido ?? ""));
                    }
                    break;

                case "tool":
                    mensajes.Add(ChatMessage.CreateToolMessage(
                        msg.IdLlamadaHerramienta ?? "",
                        msg.Contenido ?? ""));
                    break;
            }
        }

        return mensajes;
    }
}
