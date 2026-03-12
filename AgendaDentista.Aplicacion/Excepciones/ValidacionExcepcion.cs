namespace AgendaDentista.Aplicacion.Excepciones;

public class ValidacionExcepcion : Exception
{
    public ValidacionExcepcion(string mensaje) : base(mensaje) { }
}
