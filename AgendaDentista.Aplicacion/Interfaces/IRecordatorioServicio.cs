namespace AgendaDentista.Aplicacion.Interfaces;

public interface IRecordatorioServicio
{
    Task ProcesarRecordatoriosPendientesAsync();
    Task ReintentarRecordatoriosFallidosAsync();
}
