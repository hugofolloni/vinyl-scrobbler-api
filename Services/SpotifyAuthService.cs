using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace ScrobblerApi.Services;

public class SpotifyAuthService
{
    private readonly SpotifySettings _settings;
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public SpotifyAuthService(IOptions<SpotifySettings> options, HttpClient httpClient)
    {
        _settings = options.Value;
        _httpClient = httpClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return _accessToken;

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        int expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Refresh 60s early

        return _accessToken!;
    }
}