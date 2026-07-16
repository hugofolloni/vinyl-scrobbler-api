namespace ScrobblerApi.Models.DTOs;

public class WebScrobbleRequest
{
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string SessionKey { get; set; } = string.Empty;
}