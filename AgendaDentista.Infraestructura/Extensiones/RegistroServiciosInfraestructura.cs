using AgendaDentista.Aplicacion.Configuracion;
using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Configuracion;
using AgendaDentista.Infraestructura.Datos;
using AgendaDentista.Infraestructura.Repositorios;
using AgendaDentista.Infraestructura.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaDentista.Infraestructura.Extensiones;

public static class RegistroServiciosInfraestructura
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AgendaDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDentistaRepositorio, DentistaRepositorio>();
        services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
        services.AddScoped<ICitaRepositorio, CitaRepositorio>();
        services.AddScoped<IRecordatorioRepositorio, RecordatorioRepositorio>();
        services.AddScoped<IMensajeWhatsAppRepositorio, MensajeWhatsAppRepositorio>();
        services.AddScoped<ILogSistemaRepositorio, LogSistemaRepositorio>();
        services.AddScoped<IConversacionRepositorio, ConversacionRepositorio>();

        services.Configure<WhatsAppConfiguracion>(configuration.GetSection("WhatsApp"));
        services.Configure<ChatbotConfiguracion>(configuration.GetSection("Chatbot"));
        services.AddHttpClient<IWhatsAppServicio, WhatsAppCloudApiServicio>();
        services.AddScoped<ILlmServicio, OpenAILlmServicio>();

        return services;
    }
}
