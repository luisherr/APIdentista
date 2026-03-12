using System.Net;
using System.Text.Json;
using AgendaDentista.Aplicacion.Excepciones;

namespace AgendaDentista.API.Middleware;

public class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _logger;

    public ManejadorExcepcionesMiddleware(RequestDelegate next, ILogger<ManejadorExcepcionesMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado");
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private static async Task ManejarExcepcionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensaje) = exception switch
        {
            EntidadNoEncontradaExcepcion => (HttpStatusCode.NotFound, exception.Message),
            ValidacionExcepcion => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            WhatsAppExcepcion => (HttpStatusCode.BadGateway, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error interno en el servidor.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { error = mensaje });
        await context.Response.WriteAsync(response);
    }
}
