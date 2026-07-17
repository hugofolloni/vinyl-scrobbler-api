using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;
using System.Text.Json;
using ScrobblerApi.Models.DTOs;
using System.Net.Http.Headers;
using ScrobblerApi.Services;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly LastFmSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly SpotifyAuthService _spotifyAuth;
    private readonly AlbumSettings _albumSettings;
    private readonly LastFmSettings _lastFmSettings;
    public AlbumController(IOptions<LastFmSettings> lastFmOptions, IOptions<AlbumSettings> albumOptions, HttpClient httpClient, SpotifyAuthService spotifyAuth)
    {
        _lastFmSettings = lastFmOptions.Value;
        _albumSettings = albumOptions.Value;
        _settings = lastFmOptions.Value;
        _httpClient = httpClient;
        _spotifyAuth = spotifyAuth;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q)) return Ok(new List<AlbumSearchResult>());

        if (_albumSettings.Provider?.ToLower() == "spotify")
        {
            var token = await _spotifyAuth.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var spotifyUrl = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(q)}&type=album&limit=10";
            var response = await _httpClient.GetAsync(spotifyUrl);
            if (!response.IsSuccessStatusCode) return BadRequest("Error searching Spotify.");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var results = new List<AlbumSearchResult>();

            if (doc.RootElement.TryGetProperty("albums", out var albums) && albums.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var albumId = item.GetProperty("id").GetString();
                    var album = new AlbumSearchResult { Name = item.GetProperty("name").GetString() ?? "", Artist = item.GetProperty("artists")[0].GetProperty("name").GetString() ?? "", ImageUrl = item.GetProperty("images")[0].GetProperty("url").GetString() ?? "", Popularity = 0 };
                    var detailUrl = $"https://api.spotify.com/v1/albums/{albumId}";
                    var detailResponse = await _httpClient.GetAsync(detailUrl);
                    if (detailResponse.IsSuccessStatusCode) { var detailContent = await detailResponse.Content.ReadAsStringAsync(); using var detailDoc = JsonDocument.Parse(detailContent); album.Popularity = detailDoc.RootElement.TryGetProperty("popularity", out var p) ? p.GetInt32() : 0; }
                    results.Add(album);
                }
            }
            string query = q.ToLower();
            return Ok(results.OrderByDescending(x => { double score = x.Popularity; if (x.Name.ToLower().Contains(query)) score += 100; if (x.Name.ToLower() == query) score += 200; return score; }).ToList());
        }
        else
        {
            var url = $"{_lastFmSettings.BaseUrl}?method=album.search&album={Uri.EscapeDataString(q)}&api_key={_lastFmSettings.ApiKey}&format=json";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return BadRequest();
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var results = new List<AlbumSearchResult>();
            if (doc.RootElement.TryGetProperty("results", out var resultsProp) && resultsProp.TryGetProperty("albummatches", out var matchesProp) && matchesProp.TryGetProperty("album", out var albumArray))
            {
                foreach (var album in albumArray.EnumerateArray())
                {
                    var item = new AlbumSearchResult { Name = album.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "", Artist = album.TryGetProperty("artist", out var a) ? a.GetString() ?? "" : "", ImageUrl = "" };
                    if (album.TryGetProperty("image", out var imgArray) && imgArray.GetArrayLength() > 2) item.ImageUrl = imgArray[2].TryGetProperty("#text", out var t) ? t.GetString() ?? "" : "";
                    results.Add(item);
                }
            }
            return Ok(results);
        }
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetInfo([FromQuery] string artist, [FromQuery] string album)
    {
        var url = $"{_settings.BaseUrl}?method=album.getinfo&api_key={_settings.ApiKey}&artist={Uri.EscapeDataString(artist)}&album={Uri.EscapeDataString(album)}&format=json";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode) return BadRequest();

        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var albumResult = new AlbumInfoDto { Album = album, Artist = artist };

        if (doc.RootElement.TryGetProperty("album", out var albumProp))
        {
            if (albumProp.TryGetProperty("image", out var images) && images.GetArrayLength() > 2)
            {
                albumResult.ImageUrl = images[2].GetProperty("#text").GetString() ?? "";
            }

            if (albumProp.TryGetProperty("tracks", out var tracksProp) && tracksProp.TryGetProperty("track", out var trackArray))
            {
                foreach (var track in trackArray.EnumerateArray())
                {
                    var trackDto = new TrackDto();
                    trackDto.Name = track.TryGetProperty("name", out var n) ? n.GetString() ?? "Faixa Desconhecida" : "Faixa Desconhecida";
                    
                    if (track.TryGetProperty("duration", out var dProp))
                    {
                        if (dProp.ValueKind == JsonValueKind.Number) 
                            trackDto.DurationInSeconds = dProp.GetInt32();
                        else if (dProp.ValueKind == JsonValueKind.String && int.TryParse(dProp.GetString(), out int d)) 
                            trackDto.DurationInSeconds = d;
                    }
                    
                    albumResult.Tracks.Add(trackDto);
                }
            }
        }

        return Ok(albumResult);
    }
}