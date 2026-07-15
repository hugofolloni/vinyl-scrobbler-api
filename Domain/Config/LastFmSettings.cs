namespace ScrobblerApi.Models.Config;

public class LastFmSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SharedSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string SessionKey { get; set; } = string.Empty;
}