using AgendaDentista.Aplicacion.DTOs.Cita;
using AgendaDentista.Aplicacion.DTOs.Dentista;
using AgendaDentista.Aplicacion.DTOs.Paciente;
using AgendaDentista.Aplicacion.DTOs.Recordatorio;
using AgendaDentista.Dominio.Entidades;

namespace AgendaDentista.Aplicacion.Mapeos;

public static class MapeosExtensiones
{
    public static DentistaDto ToDto(this Dentista entidad) => new()
    {
        IdDentista = entidad.IdDentista,
        Nombre = entidad.Nombre,
        Telefono = entidad.Telefono,
        Email = entidad.Email,
        FechaRegistro = entidad.FechaRegistro,
        Activo = entidad.Activo
    };

    public static PacienteDto ToDto(this Paciente entidad) => new()
    {
        IdPaciente = entidad.IdPaciente,
        IdDentista = entidad.IdDentista,
        Nombre = entidad.Nombre,
        Telefono = entidad.Telefono,
        Email = entidad.Email,
        FechaRegistro = entidad.FechaRegistro,
        Activo = entidad.Activo
    };

    public static CitaDto ToDto(this Cita entidad) => new()
    {
        IdCita = entidad.IdCita,
        IdPaciente = entidad.IdPaciente,
        IdDentista = entidad.IdDentista,
        NombrePaciente = entidad.Paciente?.Nombre ?? string.Empty,
        NombreDentista = entidad.Dentista?.Nombre ?? string.Empty,
        FechaHora = entidad.FechaHora,
        Tratamiento = entidad.Tratamiento,
        Estado = entidad.Estado,
        Confirmado = entidad.Confirmado,
        RecordatorioEnviado = entidad.RecordatorioEnviado,
        FechaCreacion = entidad.FechaCreacion,
        FechaActualizacion = entidad.FechaActualizacion
    };

    public static RecordatorioDto ToDto(this Dominio.Entidades.Recordatorio entidad) => new()
    {
        IdRecordatorio = entidad.IdRecordatorio,
        IdCita = entidad.IdCita,
        FechaEnvio = entidad.FechaEnvio,
        TipoRecordatorio = entidad.TipoRecordatorio,
        EstadoEnvio = entidad.EstadoEnvio,
        Intentos = entidad.Intentos,
        RespuestaPaciente = entidad.RespuestaPaciente
    };
}
