using System.Text.Json;
using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;

namespace ScrobblerApi.Services;

public class ScrobbleService
{
    private readonly LastFmSettings _settings;
    private readonly LastFmAuthService _authService;
    private readonly HttpClient _httpClient;

    public ScrobbleService(IOptions<LastFmSettings> options, LastFmAuthService authService, HttpClient httpClient)
    {
        _settings = options.Value;
        _authService = authService;
        _httpClient = httpClient;
    }

    public async Task<bool> ScrobbleAlbumAsync(string artist, string albumName, long startUnixTimestamp, string sessionKey)
    {
        var getInfoUrl = $"{_settings.BaseUrl}?method=album.getinfo&api_key={_settings.ApiKey}&artist={artist}&album={albumName}&format=json";
        var infoResponse = await _httpClient.GetAsync(getInfoUrl);
        
        if (!infoResponse.IsSuccessStatusCode) return false;

        var infoContent = await infoResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(infoContent);

        if (!jsonDoc.RootElement.TryGetProperty("album", out var albumProp) ||
            !albumProp.TryGetProperty("tracks", out var tracksProp) ||
            !tracksProp.TryGetProperty("track", out var trackArray))
        {
            Console.WriteLine("\n[ERRO] Álbum não encontrado ou sem faixas no Last.fm.");
            return false;
        }

        var parameters = new Dictionary<string, string>
        {
            { "api_key", _settings.ApiKey },
            { "method", "track.scrobble" },
            { "sk", sessionKey }
        };

        long currentTimestamp = startUnixTimestamp;
        int index = 0;

        foreach (var track in trackArray.EnumerateArray())
        {
            if (index >= 50) break;

            string trackName = track.GetProperty("name").GetString() ?? "Unknown Track";
            
            int duration = 180;
            if (track.TryGetProperty("duration", out var durationProp))
            {
                if (durationProp.ValueKind == JsonValueKind.Number)
                    duration = durationProp.GetInt32();
                else if (durationProp.ValueKind == JsonValueKind.String && int.TryParse(durationProp.GetString(), out int d))
                    duration = d;
            }
            if (duration <= 0) duration = 180;

            parameters.Add($"album[{index}]", albumName);
            parameters.Add($"artist[{index}]", artist);
            parameters.Add($"track[{index}]", trackName);
            parameters.Add($"timestamp[{index}]", currentTimestamp.ToString());

            currentTimestamp += duration; 
            index++;
        }

        if (index == 0) return false;

        var apiSig = _authService.GenerateApiSignature(parameters);
        parameters.Add("api_sig", apiSig);
        parameters.Add("format", "json");

        var formContent = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(_settings.BaseUrl, formContent);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"\n{index} faixas do disco '{albumName}' foram registadas com sucesso.");
            return true;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"\n[ERRO NO SCROBBLE DE ÁLBUM] {errorContent}");
        return false;
    }
}