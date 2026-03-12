using System.Text.RegularExpressions;

namespace AgendaDentista.Dominio.Utilidades;

public static class NormalizadorTelefono
{
    public static string Normalizar(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            throw new ArgumentException("El teléfono no puede estar vacío.");

        // Remover espacios, guiones, paréntesis y puntos
        var limpio = Regex.Replace(telefono.Trim(), @"[\s\-\(\)\.]", "");

        // Remover el prefijo +521 -> +52 (el 1 de larga distancia ya no se usa en México)
        if (Regex.IsMatch(limpio, @"^\+521\d{10}$"))
            return "+52" + limpio[4..];

        // Si ya tiene formato +52 seguido de 10 dígitos
        if (Regex.IsMatch(limpio, @"^\+52\d{10}$"))
            return limpio;

        // Si tiene 521 al inicio (sin +) seguido de 10 dígitos
        if (Regex.IsMatch(limpio, @"^521\d{10}$"))
            return "+52" + limpio[3..];

        // Si tiene 52 al inicio (sin +) seguido de 10 dígitos
        if (Regex.IsMatch(limpio, @"^52\d{10}$"))
            return "+52" + limpio[2..];

        // Si son solo 10 dígitos (número local mexicano)
        if (Regex.IsMatch(limpio, @"^\d{10}$"))
            return "+52" + limpio;

        throw new ArgumentException($"Formato de teléfono no válido: {telefono}");
    }
}
