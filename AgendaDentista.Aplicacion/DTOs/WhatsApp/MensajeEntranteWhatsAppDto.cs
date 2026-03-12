using System.Text.Json.Serialization;

namespace AgendaDentista.Aplicacion.DTOs.WhatsApp;

public class MensajeEntranteWhatsAppDto
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("entry")]
    public List<EntryDto>? Entry { get; set; }
}

public class EntryDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("changes")]
    public List<ChangeDto>? Changes { get; set; }
}

public class ChangeDto
{
    [JsonPropertyName("value")]
    public ValueDto? Value { get; set; }
}

public class ValueDto
{
    [JsonPropertyName("messaging_product")]
    public string? MessagingProduct { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageDto>? Messages { get; set; }

    [JsonPropertyName("contacts")]
    public List<ContactDto>? Contacts { get; set; }
}

public class MessageDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public TextDto? Text { get; set; }
}

public class TextDto
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class ContactDto
{
    [JsonPropertyName("wa_id")]
    public string? WaId { get; set; }

    [JsonPropertyName("profile")]
    public ProfileDto? Profile { get; set; }
}

public class ProfileDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
