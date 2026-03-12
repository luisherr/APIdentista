using AgendaDentista.Aplicacion.Extensiones;
using AgendaDentista.Infraestructura.Extensiones;
using AgendaDentista.WorkerRecordatorios;
using AgendaDentista.WorkerRecordatorios.Configuracion;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerConfiguracion>(builder.Configuration.GetSection("Worker"));
builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
