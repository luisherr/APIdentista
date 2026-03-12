using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaDentista.Aplicacion.Extensiones;

public static class RegistroServiciosAplicacion
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IDentistaServicio, DentistaServicio>();
        services.AddScoped<IPacienteServicio, PacienteServicio>();
        services.AddScoped<ICitaServicio, CitaServicio>();
        services.AddScoped<IRecordatorioServicio, RecordatorioServicio>();
        services.AddScoped<IAuthServicio, AuthServicio>();
        services.AddScoped<IChatbotServicio, ChatbotServicio>();

        return services;
    }
}
