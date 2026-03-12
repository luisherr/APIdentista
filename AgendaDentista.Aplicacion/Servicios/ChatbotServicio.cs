using System.Text.Json;
using AgendaDentista.Aplicacion.DTOs.Chatbot;
using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Aplicacion.DTOs.Paciente;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Dominio.Utilidades;
using AgendaDentista.Aplicacion.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgendaDentista.Aplicacion.Servicios;

public class ChatbotServicio : IChatbotServicio
{
    private readonly ILlmServicio _llmServicio;
    private readonly IConversacionRepositorio _conversacionRepositorio;
    private readonly IPacienteServicio _pacienteServicio;
    private readonly ICitaServicio _citaServicio;
    private readonly ICitaRepositorio _citaRepositorio;
    private readonly IDentistaRepositorio _dentistaRepositorio;
    private readonly ChatbotConfiguracion _config;
    private readonly ILogger<ChatbotServicio> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ChatbotServicio(
        ILlmServicio llmServicio,
        IConversacionRepositorio conversacionRepositorio,
        IPacienteServicio pacienteServicio,
        ICitaServicio citaServicio,
        ICitaRepositorio citaRepositorio,
        IDentistaRepositorio dentistaRepositorio,
        IOptions<ChatbotConfiguracion> config,
        ILogger<ChatbotServicio> logger)
    {
        _llmServicio = llmServicio;
        _conversacionRepositorio = conversacionRepositorio;
        _pacienteServicio = pacienteServicio;
        _citaServicio = citaServicio;
        _citaRepositorio = citaRepositorio;
        _dentistaRepositorio = dentistaRepositorio;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> ProcesarMensajeAsync(string telefono, string mensaje, string? nombrePerfil = null)
    {
        try
        {
            var telefonoNormalizado = NormalizadorTelefono.Normalizar(telefono);
            var conversacion = await ObtenerOCrearConversacionAsync(telefonoNormalizado, nombrePerfil);
            var historial = DeserializarHistorial(conversacion.HistorialMensajesJson);

            // Agregar mensaje del usuario
            historial.Add(new MensajeLlm { Rol = "user", Contenido = mensaje });

            // Trim historial si excede el limite
            TrimHistorial(historial);

            var herramientas = ObtenerDefinicionesHerramientas();
            var respuestaFinal = "";
            var iteraciones = 0;
            const int maxIteraciones = 5;

            // Loop de function calling
            while (iteraciones < maxIteraciones)
            {
                iteraciones++;
                var respuesta = await _llmServicio.ObtenerRespuestaAsync(historial, herramientas);

                if (!respuesta.RequiereHerramientas)
                {
                    respuestaFinal = respuesta.Contenido ?? "Lo siento, no pude procesar tu mensaje.";
                    historial.Add(new MensajeLlm { Rol = "assistant", Contenido = respuestaFinal });
                    break;
                }

                // Agregar mensaje del asistente con tool calls
                historial.Add(new MensajeLlm
                {
                    Rol = "assistant",
                    Contenido = respuesta.Contenido,
                    LlamadasHerramienta = respuesta.LlamadasHerramienta
                });

                // Ejecutar cada herramienta
                foreach (var llamada in respuesta.LlamadasHerramienta!)
                {
                    var resultado = await EjecutarHerramientaAsync(llamada, conversacion);
                    historial.Add(new MensajeLlm
                    {
                        Rol = "tool",
                        Contenido = resultado,
                        IdLlamadaHerramienta = llamada.Id,
                        NombreHerramienta = llamada.Nombre
                    });
                }
            }

            if (iteraciones >= maxIteraciones && string.IsNullOrEmpty(respuestaFinal))
            {
                respuestaFinal = "Disculpa, tuve un problema procesando tu solicitud. ¿Podrías intentar de nuevo?";
                historial.Add(new MensajeLlm { Rol = "assistant", Contenido = respuestaFinal });
            }

            // Guardar historial actualizado
            conversacion.HistorialMensajesJson = SerializarHistorial(historial);
            conversacion.UltimaActividad = DateTime.UtcNow;
            await _conversacionRepositorio.ActualizarAsync(conversacion);

            return respuestaFinal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando mensaje del chatbot para {Telefono}", telefono);
            return "Disculpa, estoy teniendo problemas técnicos. Por favor intenta de nuevo en unos minutos.";
        }
    }

    private async Task<ConversacionWhatsApp> ObtenerOCrearConversacionAsync(string telefono, string? nombrePerfil)
    {
        var conversacion = await _conversacionRepositorio.ObtenerActivaPorTelefonoAsync(telefono);

        if (conversacion != null)
        {
            // Verificar si la sesion expiro
            var minutosInactivo = (DateTime.UtcNow - conversacion.UltimaActividad).TotalMinutes;
            if (minutosInactivo > _config.SesionExpirarMinutos)
            {
                conversacion.Activa = false;
                await _conversacionRepositorio.ActualizarAsync(conversacion);
                conversacion = null;
            }
        }

        if (conversacion != null)
            return conversacion;

        // Crear nueva conversacion
        var paciente = await _pacienteServicio.ObtenerPorTelefonoAsync(telefono);
        var dentista = await _dentistaRepositorio.ObtenerPorIdAsync(_config.IdDentistaDefault);

        var systemPrompt = ConstruirSystemPrompt(paciente, dentista, nombrePerfil);

        var historialInicial = new List<MensajeLlm>
        {
            new() { Rol = "system", Contenido = systemPrompt }
        };

        var nuevaConversacion = new ConversacionWhatsApp
        {
            Telefono = telefono,
            IdPaciente = paciente != null ? int.Parse(paciente.IdPaciente.ToString()) : null,
            IdDentista = _config.IdDentistaDefault,
            HistorialMensajesJson = SerializarHistorial(historialInicial),
            UltimaActividad = DateTime.UtcNow,
            Activa = true
        };

        return await _conversacionRepositorio.AgregarAsync(nuevaConversacion);
    }

    private string ConstruirSystemPrompt(PacienteDto? paciente, Dentista? dentista, string? nombrePerfil)
    {
        var nombreDentista = dentista?.Nombre ?? "el dentista";
        var precios = string.Join("\n", _config.ListaPrecios.Select(p => $"- {p.Tratamiento}: {p.PrecioAproximado}"));

        var infoPaciente = paciente != null
            ? $"El paciente ya está registrado. Nombre: {paciente.Nombre}, Teléfono: {paciente.Telefono}, ID: {paciente.IdPaciente}."
            : $"El paciente NO está registrado aún. Su nombre de perfil de WhatsApp es: {nombrePerfil ?? "desconocido"}. Si quiere agendar una cita, primero debes registrarlo usando la herramienta registrar_paciente.";

        return $"""
            Eres el asistente virtual del consultorio dental del Dr./Dra. {nombreDentista}. Respondes por WhatsApp en español de manera amable, profesional y concisa.

            INFORMACIÓN DEL PACIENTE:
            {infoPaciente}

            LISTA DE PRECIOS APROXIMADOS:
            {precios}

            REGLAS:
            1. Mantén tus respuestas cortas (máximo 500 caracteres) ya que es WhatsApp.
            2. Usa emojis moderadamente para ser amigable (🦷, ✅, 📅, etc).
            3. Siempre confirma los datos antes de agendar una cita (fecha, hora, tratamiento).
            4. Las citas se agendan con el dentista ID {_config.IdDentistaDefault}.
            5. Si el paciente no está registrado y quiere agendar, primero pídele su nombre completo para registrarlo.
            6. Para fechas, usa formato claro. Pregunta fecha y hora específica.
            7. Si el paciente pregunta por precios, muestra la lista de precios.
            8. Para cancelar o reprogramar, primero consulta las citas del paciente.
            9. Sé empático y profesional. No des diagnósticos médicos.
            10. Si no entiendes algo, pide que lo reformulen amablemente.
            11. Cuando agendes una cita exitosamente, confirma fecha, hora y tratamiento.
            12. La fecha y hora actual es: {DateTime.Now:yyyy-MM-dd HH:mm}. Usa esto como referencia para citas futuras.
            """;
    }

    private async Task<string> EjecutarHerramientaAsync(LlamadaHerramienta llamada, ConversacionWhatsApp conversacion)
    {
        try
        {
            var args = JsonDocument.Parse(llamada.ArgumentosJson).RootElement;

            return llamada.Nombre switch
            {
                "buscar_paciente" => await EjecutarBuscarPacienteAsync(args),
                "registrar_paciente" => await EjecutarRegistrarPacienteAsync(args, conversacion),
                "agendar_cita" => await EjecutarAgendarCitaAsync(args),
                "obtener_citas_paciente" => await EjecutarObtenerCitasAsync(args),
                "cancelar_cita" => await EjecutarCancelarCitaAsync(args),
                "reprogramar_cita" => await EjecutarReprogramarCitaAsync(args),
                "obtener_precios" => EjecutarObtenerPrecios(),
                _ => JsonSerializer.Serialize(new { error = $"Herramienta '{llamada.Nombre}' no encontrada." })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando herramienta {Herramienta}", llamada.Nombre);
            return JsonSerializer.Serialize(new { error = $"Error al ejecutar {llamada.Nombre}: {ex.Message}" });
        }
    }

    private async Task<string> EjecutarBuscarPacienteAsync(JsonElement args)
    {
        var telefono = args.GetProperty("telefono").GetString() ?? "";
        var paciente = await _pacienteServicio.ObtenerPorTelefonoAsync(telefono);

        if (paciente == null)
            return JsonSerializer.Serialize(new { encontrado = false, mensaje = "Paciente no encontrado." }, _jsonOptions);

        return JsonSerializer.Serialize(new
        {
            encontrado = true,
            idPaciente = paciente.IdPaciente,
            nombre = paciente.Nombre,
            telefono = paciente.Telefono,
            email = paciente.Email
        }, _jsonOptions);
    }

    private async Task<string> EjecutarRegistrarPacienteAsync(JsonElement args, ConversacionWhatsApp conversacion)
    {
        var nombre = args.GetProperty("nombre").GetString() ?? "";
        var telefono = conversacion.Telefono;
        string? email = args.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;

        var dto = new CrearPacienteDto
        {
            IdDentista = _config.IdDentistaDefault,
            Nombre = nombre,
            Telefono = telefono,
            Email = email
        };

        var paciente = await _pacienteServicio.CrearPacienteAsync(dto);

        // Actualizar la conversacion con el ID del paciente
        conversacion.IdPaciente = paciente.IdPaciente;

        return JsonSerializer.Serialize(new
        {
            exito = true,
            idPaciente = paciente.IdPaciente,
            nombre = paciente.Nombre,
            mensaje = "Paciente registrado exitosamente."
        }, _jsonOptions);
    }

    private async Task<string> EjecutarAgendarCitaAsync(JsonElement args)
    {
        var idPaciente = args.GetProperty("idPaciente").GetInt32();
        var fechaHoraStr = args.GetProperty("fechaHora").GetString() ?? "";
        var tratamiento = args.GetProperty("tratamiento").GetString() ?? "";

        if (!DateTime.TryParse(fechaHoraStr, out var fechaHora))
            return JsonSerializer.Serialize(new { error = "Formato de fecha inválido. Usa formato: 2024-03-15 10:00" }, _jsonOptions);

        var dto = new CrearCitaDto
        {
            IdPaciente = idPaciente,
            IdDentista = _config.IdDentistaDefault,
            FechaHora = fechaHora,
            Tratamiento = tratamiento
        };

        var cita = await _citaServicio.CrearCitaAsync(dto);

        return JsonSerializer.Serialize(new
        {
            exito = true,
            idCita = cita.IdCita,
            fechaHora = cita.FechaHora.ToString("yyyy-MM-dd HH:mm"),
            tratamiento = cita.Tratamiento,
            estado = cita.Estado.ToString(),
            mensaje = "Cita agendada exitosamente."
        }, _jsonOptions);
    }

    private async Task<string> EjecutarObtenerCitasAsync(JsonElement args)
    {
        var idPaciente = args.GetProperty("idPaciente").GetInt32();
        var citas = await _citaRepositorio.ObtenerCitasPorPacienteAsync(idPaciente);

        var citasFuturas = citas
            .Where(c => c.FechaHora > DateTime.UtcNow && c.Estado != EstadoCita.Cancelada)
            .Select(c => new
            {
                idCita = c.IdCita,
                fechaHora = c.FechaHora.ToString("yyyy-MM-dd HH:mm"),
                tratamiento = c.Tratamiento,
                estado = c.Estado.ToString()
            })
            .ToList();

        if (!citasFuturas.Any())
            return JsonSerializer.Serialize(new { citas = Array.Empty<object>(), mensaje = "No hay citas futuras programadas." }, _jsonOptions);

        return JsonSerializer.Serialize(new { citas = citasFuturas }, _jsonOptions);
    }

    private async Task<string> EjecutarCancelarCitaAsync(JsonElement args)
    {
        var idCita = args.GetProperty("idCita").GetInt32();
        var cita = await _citaServicio.CancelarCitaAsync(idCita);

        return JsonSerializer.Serialize(new
        {
            exito = true,
            idCita = cita.IdCita,
            estado = cita.Estado.ToString(),
            mensaje = "Cita cancelada exitosamente."
        }, _jsonOptions);
    }

    private async Task<string> EjecutarReprogramarCitaAsync(JsonElement args)
    {
        var idCitaAnterior = args.GetProperty("idCitaAnterior").GetInt32();
        var nuevaFechaStr = args.GetProperty("nuevaFechaHora").GetString() ?? "";
        var tratamiento = args.TryGetProperty("tratamiento", out var tratProp) ? tratProp.GetString() : null;

        // Cancelar cita anterior
        var citaCancelada = await _citaServicio.CancelarCitaAsync(idCitaAnterior);

        if (!DateTime.TryParse(nuevaFechaStr, out var nuevaFecha))
            return JsonSerializer.Serialize(new { error = "Formato de fecha inválido para la nueva cita." }, _jsonOptions);

        // Crear nueva cita
        var dto = new CrearCitaDto
        {
            IdPaciente = citaCancelada.IdPaciente,
            IdDentista = citaCancelada.IdDentista,
            FechaHora = nuevaFecha,
            Tratamiento = tratamiento ?? citaCancelada.Tratamiento
        };

        var nuevaCita = await _citaServicio.CrearCitaAsync(dto);

        return JsonSerializer.Serialize(new
        {
            exito = true,
            citaCancelada = idCitaAnterior,
            nuevaCita = new
            {
                idCita = nuevaCita.IdCita,
                fechaHora = nuevaCita.FechaHora.ToString("yyyy-MM-dd HH:mm"),
                tratamiento = nuevaCita.Tratamiento
            },
            mensaje = "Cita reprogramada exitosamente."
        }, _jsonOptions);
    }

    private string EjecutarObtenerPrecios()
    {
        var precios = _config.ListaPrecios.Select(p => new
        {
            tratamiento = p.Tratamiento,
            precioAproximado = p.PrecioAproximado
        }).ToList();

        return JsonSerializer.Serialize(new { precios }, _jsonOptions);
    }

    private static List<DefinicionHerramienta> ObtenerDefinicionesHerramientas()
    {
        return new List<DefinicionHerramienta>
        {
            new()
            {
                Nombre = "buscar_paciente",
                Descripcion = "Busca un paciente registrado por su número de teléfono.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "telefono": { "type": "string", "description": "Número de teléfono del paciente" }
                    },
                    "required": ["telefono"]
                }
                """
            },
            new()
            {
                Nombre = "registrar_paciente",
                Descripcion = "Registra un nuevo paciente en el sistema. Usa el teléfono de la conversación actual.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "nombre": { "type": "string", "description": "Nombre completo del paciente" },
                        "email": { "type": "string", "description": "Email del paciente (opcional)" }
                    },
                    "required": ["nombre"]
                }
                """
            },
            new()
            {
                Nombre = "agendar_cita",
                Descripcion = "Agenda una nueva cita dental para un paciente registrado.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "idPaciente": { "type": "integer", "description": "ID del paciente" },
                        "fechaHora": { "type": "string", "description": "Fecha y hora de la cita en formato 'yyyy-MM-dd HH:mm'" },
                        "tratamiento": { "type": "string", "description": "Tipo de tratamiento dental" }
                    },
                    "required": ["idPaciente", "fechaHora", "tratamiento"]
                }
                """
            },
            new()
            {
                Nombre = "obtener_citas_paciente",
                Descripcion = "Obtiene las citas futuras programadas de un paciente.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "idPaciente": { "type": "integer", "description": "ID del paciente" }
                    },
                    "required": ["idPaciente"]
                }
                """
            },
            new()
            {
                Nombre = "cancelar_cita",
                Descripcion = "Cancela una cita existente.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "idCita": { "type": "integer", "description": "ID de la cita a cancelar" }
                    },
                    "required": ["idCita"]
                }
                """
            },
            new()
            {
                Nombre = "reprogramar_cita",
                Descripcion = "Reprograma una cita existente cancelándola y creando una nueva.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {
                        "idCitaAnterior": { "type": "integer", "description": "ID de la cita a reprogramar" },
                        "nuevaFechaHora": { "type": "string", "description": "Nueva fecha y hora en formato 'yyyy-MM-dd HH:mm'" },
                        "tratamiento": { "type": "string", "description": "Tipo de tratamiento (opcional, se mantiene el anterior si no se especifica)" }
                    },
                    "required": ["idCitaAnterior", "nuevaFechaHora"]
                }
                """
            },
            new()
            {
                Nombre = "obtener_precios",
                Descripcion = "Obtiene la lista de precios aproximados de los tratamientos dentales.",
                ParametrosJson = """
                {
                    "type": "object",
                    "properties": {},
                    "required": []
                }
                """
            }
        };
    }

    private void TrimHistorial(List<MensajeLlm> historial)
    {
        if (historial.Count <= _config.MaxMensajesHistorial)
            return;

        // Mantener el system prompt (primer mensaje) y los mas recientes
        var systemPrompt = historial[0];
        var mensajesRecientes = historial.Skip(historial.Count - (_config.MaxMensajesHistorial - 1)).ToList();
        historial.Clear();
        historial.Add(systemPrompt);
        historial.AddRange(mensajesRecientes);
    }

    private static List<MensajeLlm> DeserializarHistorial(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<MensajeLlm>>(json, _jsonOptions) ?? new List<MensajeLlm>();
        }
        catch
        {
            return new List<MensajeLlm>();
        }
    }

    private static string SerializarHistorial(List<MensajeLlm> historial)
    {
        return JsonSerializer.Serialize(historial, _jsonOptions);
    }
}
