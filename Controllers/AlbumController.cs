using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;
using System.Text.Json;
using ScrobblerApi.Models.DTOs;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumController : ControllerBase
{
    private readonly LastFmSettings _settings;
    private readonly HttpClient _httpClient;

    public AlbumController(IOptions<LastFmSettings> options, HttpClient httpClient)
    {
        _settings = options.Value;
        _httpClient = httpClient;
    }

   [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
{
    if (string.IsNullOrEmpty(q)) return Ok(new List<AlbumSearchResult>());

    var url = $"{_settings.BaseUrl}?method=album.search&album={Uri.EscapeDataString(q)}&api_key={_settings.ApiKey}&format=json";
    var response = await _httpClient.GetAsync(url);
    if (!response.IsSuccessStatusCode) return BadRequest();

    var content = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(content);
    
    var results = new List<AlbumSearchResult>();
    
    if (doc.RootElement.TryGetProperty("results", out var resultsProp) &&
        resultsProp.TryGetProperty("albummatches", out var matchesProp) &&
        matchesProp.TryGetProperty("album", out var albumArray))
    {
        foreach (var album in albumArray.EnumerateArray())
        {
            var item = new AlbumSearchResult
            {
                Name = album.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                Artist = album.TryGetProperty("artist", out var a) ? a.GetString() ?? "" : "",
                ImageUrl = ""
            };
            
            if (album.TryGetProperty("image", out var imgArray) && imgArray.GetArrayLength() > 2)
            {
                item.ImageUrl = imgArray[2].TryGetProperty("#text", out var t) ? t.GetString() ?? "" : "";
            }
            results.Add(item);
        }
    }

    return Ok(results);
    }

    [HttpGet("info")]
public async Task<IActionResult> GetInfo([FromQuery] string artist, [FromQuery] string album)
    {
        if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
            return BadRequest(new { mensagem = "Artista e Álbum são obrigatórios." });

        var url = $"{_settings.BaseUrl}?method=album.getinfo&api_key={_settings.ApiKey}&artist={Uri.EscapeDataString(artist)}&album={Uri.EscapeDataString(album)}&format=json";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode) return BadRequest();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        
        var albumResult = new AlbumInfoDto { Album = album, Artist = artist };

        if (doc.RootElement.TryGetProperty("album", out var albumProp))
        {
            // Extrair Imagem
            if (albumProp.TryGetProperty("image", out var images) && images.GetArrayLength() > 2)
            {
                albumResult.ImageUrl = images[2].GetProperty("#text").GetString() ?? "";
            }

            // Extrair Tracks
            if (albumProp.TryGetProperty("tracks", out var tracksProp) &&
                tracksProp.TryGetProperty("track", out var trackArray))
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