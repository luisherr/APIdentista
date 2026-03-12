using AgendaDentista.Aplicacion.Interfaces;
using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Enums;
using AgendaDentista.Dominio.Interfaces;

namespace AgendaDentista.Aplicacion.Servicios;

public class RecordatorioServicio : IRecordatorioServicio
{
    private readonly ICitaRepositorio _citaRepositorio;
    private readonly IRecordatorioRepositorio _recordatorioRepositorio;
    private readonly IWhatsAppServicio _whatsAppServicio;
    private readonly IMensajeWhatsAppRepositorio _mensajeRepositorio;
    private readonly ILogSistemaRepositorio _logRepositorio;

    public RecordatorioServicio(
        ICitaRepositorio citaRepositorio,
        IRecordatorioRepositorio recordatorioRepositorio,
        IWhatsAppServicio whatsAppServicio,
        IMensajeWhatsAppRepositorio mensajeRepositorio,
        ILogSistemaRepositorio logRepositorio)
    {
        _citaRepositorio = citaRepositorio;
        _recordatorioRepositorio = recordatorioRepositorio;
        _whatsAppServicio = whatsAppServicio;
        _mensajeRepositorio = mensajeRepositorio;
        _logRepositorio = logRepositorio;
    }

    public async Task ProcesarRecordatoriosPendientesAsync()
    {
        var citasPendientes = await _citaRepositorio.ObtenerCitasPendientesRecordatorioAsync();

        foreach (var cita in citasPendientes)
        {
            try
            {
                var tipo = TipoRecordatorio.Horas24;
                var horasRestantes = (cita.FechaHora - DateTime.UtcNow).TotalHours;
                if (horasRestantes <= 3)
                    tipo = TipoRecordatorio.Horas3;

                if (await _recordatorioRepositorio.ExisteRecordatorioParaCitaAsync(cita.IdCita, tipo))
                    continue;

                var mensaje = GenerarMensajeRecordatorio(cita);
                var enviado = await _whatsAppServicio.EnviarMensajeAsync(cita.Paciente.Telefono, mensaje);

                var recordatorio = new Recordatorio
                {
                    IdCita = cita.IdCita,
                    FechaEnvio = DateTime.UtcNow,
                    TipoRecordatorio = tipo,
                    EstadoEnvio = enviado ? EstadoEnvio.Enviado : EstadoEnvio.Fallido,
                    Intentos = 1
                };

                await _recordatorioRepositorio.AgregarAsync(recordatorio);

                if (enviado)
                {
                    cita.RecordatorioEnviado = true;
                    cita.FechaActualizacion = DateTime.UtcNow;
                    await _citaRepositorio.ActualizarAsync(cita);
                }

                await _logRepositorio.RegistrarAsync(
                    enviado ? TipoLog.Info : TipoLog.Warning,
                    $"Recordatorio {tipo} para cita {cita.IdCita}: {(enviado ? "enviado" : "fallido")}");
            }
            catch (Exception ex)
            {
                await _logRepositorio.RegistrarAsync(TipoLog.Error,
                    $"Error procesando recordatorio para cita {cita.IdCita}: {ex.Message}",
                    ex.StackTrace);
            }
        }
    }

    public async Task ReintentarRecordatoriosFallidosAsync()
    {
        var fallidos = await _recordatorioRepositorio.ObtenerRecordatoriosFallidosAsync();

        foreach (var recordatorio in fallidos)
        {
            try
            {
                var cita = await _citaRepositorio.ObtenerPorIdAsync(recordatorio.IdCita);
                if (cita == null || cita.Estado != EstadoCita.Pendiente)
                    continue;

                var mensaje = GenerarMensajeRecordatorio(cita);
                var enviado = await _whatsAppServicio.EnviarMensajeAsync(cita.Paciente.Telefono, mensaje);

                recordatorio.Intentos++;
                recordatorio.EstadoEnvio = enviado ? EstadoEnvio.Enviado : EstadoEnvio.Fallido;
                recordatorio.FechaEnvio = DateTime.UtcNow;
                await _recordatorioRepositorio.ActualizarAsync(recordatorio);

                if (enviado)
                {
                    cita.RecordatorioEnviado = true;
                    cita.FechaActualizacion = DateTime.UtcNow;
                    await _citaRepositorio.ActualizarAsync(cita);
                }

                await _logRepositorio.RegistrarAsync(
                    enviado ? TipoLog.Info : TipoLog.Warning,
                    $"Reintento {recordatorio.Intentos} para cita {cita.IdCita}: {(enviado ? "enviado" : "fallido")}");
            }
            catch (Exception ex)
            {
                await _logRepositorio.RegistrarAsync(TipoLog.Error,
                    $"Error reintentando recordatorio {recordatorio.IdRecordatorio}: {ex.Message}",
                    ex.StackTrace);
            }
        }
    }

    private static string GenerarMensajeRecordatorio(Cita cita)
    {
        var fecha = cita.FechaHora.ToString("dd/MM/yyyy");
        var hora = cita.FechaHora.ToString("hh:mm tt");
        var nombrePaciente = cita.Paciente?.Nombre ?? "Paciente";
        var nombreDentista = cita.Dentista?.Nombre ?? "su dentista";

        return $"Hola {nombrePaciente}, te recordamos tu cita dental el {fecha} a las {hora} con {nombreDentista}.\n\n" +
               $"Tratamiento: {cita.Tratamiento}\n\n" +
               "Responde:\n" +
               "1 - Confirmar cita\n" +
               "2 - Cancelar cita";
    }
}
