namespace AgendaDentista.Aplicacion.Excepciones;

public class WhatsAppExcepcion : Exception
{
    public WhatsAppExcepcion(string mensaje) : base(mensaje) { }
    public WhatsAppExcepcion(string mensaje, Exception innerException) : base(mensaje, innerException) { }
}
