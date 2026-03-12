using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.WorkerRecordatorios.Configuracion;
using Microsoft.Extensions.Options;

namespace AgendaDentista.WorkerRecordatorios;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly WorkerConfiguracion _config;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger,
        IOptions<WorkerConfiguracion> config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de recordatorios iniciado. Intervalo: {Intervalo} minutos", _config.IntervaloMinutos);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Procesando recordatorios: {Time}", DateTimeOffset.Now);

                using var scope = _scopeFactory.CreateScope();
                var recordatorioServicio = scope.ServiceProvider.GetRequiredService<IRecordatorioServicio>();

                await recordatorioServicio.ProcesarRecordatoriosPendientesAsync();
                await recordatorioServicio.ReintentarRecordatoriosFallidosAsync();

                _logger.LogInformation("Procesamiento de recordatorios completado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del worker de recordatorios");
            }

            await Task.Delay(TimeSpan.FromMinutes(_config.IntervaloMinutos), stoppingToken);
        }
    }
}
