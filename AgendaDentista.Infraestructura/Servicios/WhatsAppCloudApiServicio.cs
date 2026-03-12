using System.Net.Http.Json;
using System.Text.Json;
using AgendaDentista.Aplicacion.DTOs.WhatsApp;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Dominio.Utilidades;
using AgendaDentista.Infraestructura.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgendaDentista.Infraestructura.Servicios;

public class WhatsAppCloudApiServicio : IWhatsAppServicio
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppConfiguracion _config;
    private readonly IMensajeWhatsAppRepositorio _mensajeRepositorio;
    private readonly IPacienteRepositorio _pacienteRepositorio;
    private readonly ILogSistemaRepositorio _logRepositorio;
    private readonly IChatbotServicio _chatbotServicio;
    private readonly ILogger<WhatsAppCloudApiServicio> _logger;

    public WhatsAppCloudApiServicio(
        HttpClient httpClient,
        IOptions<WhatsAppConfiguracion> config,
        IMensajeWhatsAppRepositorio mensajeRepositorio,
        IPacienteRepositorio pacienteRepositorio,
        ILogSistemaRepositorio logRepositorio,
        IChatbotServicio chatbotServicio,
        ILogger<WhatsAppCloudApiServicio> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _mensajeRepositorio = mensajeRepositorio;
        _pacienteRepositorio = pacienteRepositorio;
        _logRepositorio = logRepositorio;
        _chatbotServicio = chatbotServicio;
        _logger = logger;
    }

    public async Task<bool> EnviarMensajeAsync(string telefono, string mensaje)
    {
        try
        {
            var url = $"{_config.UrlBase}/{_config.NumeroTelefonoId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = telefono,
                type = "text",
                text = new { body = mensaje }
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.TokenAcceso);

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            string? idMensajeProveedor = null;
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                idMensajeProveedor = doc.RootElement
                    .GetProperty("messages")[0]
                    .GetProperty("id")
                    .GetString();
            }
            else
            {
                _logger.LogError("Meta API error {StatusCode}: {ResponseBody}",
                    (int)response.StatusCode, responseBody);
            }

            var mensajeRegistro = new MensajeWhatsApp
            {
                Telefono = telefono,
                Mensaje = mensaje,
                TipoMensaje = TipoMensaje.Enviado,
                Fecha = DateTime.UtcNow,
                EstadoEnvio = response.IsSuccessStatusCode ? EstadoEnvio.Enviado : EstadoEnvio.Fallido,
                IdMensajeProveedor = idMensajeProveedor
            };

            await _mensajeRepositorio.AgregarAsync(mensajeRegistro);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando mensaje WhatsApp a {Telefono}", telefono);
            await _logRepositorio.RegistrarAsync(TipoLog.Error,
                $"Error enviando WhatsApp a {telefono}: {ex.Message}", ex.StackTrace);
            return false;
        }
    }

    public async Task ProcesarMensajeEntranteAsync(MensajeEntranteWhatsAppDto mensajeEntrante)
    {
        var changes = mensajeEntrante.Entry?
            .SelectMany(e => e.Changes ?? Enumerable.Empty<ChangeDto>());

        if (changes == null) return;

        foreach (var change in changes)
        {
            var messages = change.Value?.Messages;
            if (messages == null) continue;

            // Extraer nombre de perfil de contactos
            var contactos = change.Value?.Contacts;

            foreach (var msg in messages)
            {
                if (msg.Type != "text" || msg.Text?.Body == null || msg.From == null)
                    continue;

                try
                {
                    var telefonoNormalizado = NormalizadorTelefono.Normalizar(msg.From);
                    var nombrePerfil = contactos?
                        .FirstOrDefault(c => c.WaId == msg.From)?
                        .Profile?.Name;

                    // Registrar mensaje entrante
                    var paciente = await _pacienteRepositorio.ObtenerPorTelefonoAsync(telefonoNormalizado);
                    var mensajeRegistro = new MensajeWhatsApp
                    {
                        IdDentista = paciente?.IdDentista ?? 0,
                        IdPaciente = paciente?.IdPaciente ?? 0,
                        Telefono = telefonoNormalizado,
                        Mensaje = msg.Text.Body,
                        TipoMensaje = TipoMensaje.Recibido,
                        Fecha = DateTime.UtcNow,
                        EstadoEnvio = EstadoEnvio.Entregado,
                        IdMensajeProveedor = msg.Id
                    };
                    await _mensajeRepositorio.AgregarAsync(mensajeRegistro);

                    // Procesar con chatbot IA
                    var respuesta = await _chatbotServicio.ProcesarMensajeAsync(
                        telefonoNormalizado, msg.Text.Body, nombrePerfil);

                    // Enviar respuesta por WhatsApp
                    await EnviarMensajeAsync(telefonoNormalizado, respuesta);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje entrante de {From}", msg.From);
                    await _logRepositorio.RegistrarAsync(TipoLog.Error,
                        $"Error procesando mensaje de {msg.From}: {ex.Message}", ex.StackTrace);
                }
            }
        }
    }
}
