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

        // Si ya tiene formato completo +521XXXXXXXXXX
        if (Regex.IsMatch(limpio, @"^\+521\d{10}$"))
            return limpio;

        // Si tiene +52 seguido de 10 dígitos (sin el 1)
        if (Regex.IsMatch(limpio, @"^\+52\d{10}$"))
            return "+521" + limpio[3..];

        // Si tiene 52 al inicio (sin +) seguido de 1 + 10 dígitos
        if (Regex.IsMatch(limpio, @"^521\d{10}$"))
            return "+" + limpio;

        // Si tiene 52 al inicio (sin +) seguido de 10 dígitos
        if (Regex.IsMatch(limpio, @"^52\d{10}$"))
            return "+521" + limpio[2..];

        // Si son solo 10 dígitos (número local)
        if (Regex.IsMatch(limpio, @"^\d{10}$"))
            return "+521" + limpio;

        throw new ArgumentException($"Formato de teléfono no válido: {telefono}");
    }
}
