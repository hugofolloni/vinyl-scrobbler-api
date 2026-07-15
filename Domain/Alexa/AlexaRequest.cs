using System.Text.Json.Serialization;

namespace ScrobblerApi.Models.DTOs;

public class AlexaRequest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("request")]
    public RequestDetails Request { get; set; } = new();
}

public class RequestDetails
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("intent")]
    public IntentDetails? Intent { get; set; }
}

public class IntentDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("confirmationStatus")]
    public string ConfirmationStatus { get; set; } = "NONE";

    [JsonPropertyName("slots")]
    public Dictionary<string, SlotDetails>? Slots { get; set; }
}

public class SlotDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}