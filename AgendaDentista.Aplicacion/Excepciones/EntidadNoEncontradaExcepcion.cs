namespace AgendaDentista.Aplicacion.Excepciones;

public class EntidadNoEncontradaExcepcion : Exception
{
    public EntidadNoEncontradaExcepcion(string entidad, int id)
        : base($"{entidad} con ID {id} no fue encontrado.") { }
}
