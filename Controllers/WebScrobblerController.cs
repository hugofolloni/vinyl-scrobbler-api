using Microsoft.AspNetCore.Mvc;
using ScrobblerApi.Services;
using ScrobblerApi.Models.DTOs;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebScrobblerController : ControllerBase
{
    private readonly ScrobbleService _scrobbleService;

    public WebScrobblerController(ScrobbleService scrobbleService)
    {
        _scrobbleService = scrobbleService;
    }

    [HttpPost("scrobble")]
    public async Task<IActionResult> ScrobbleAlbum([FromBody] WebScrobbleRequest request)
    {
        if (string.IsNullOrEmpty(request.Artist) || string.IsNullOrEmpty(request.Album))
        {
            return BadRequest(new { mensagem = "Artista e Álbum são campos obrigatórios." });
        }

        Console.WriteLine($"\n[WEB] Solicitado scrobble do álbum: {request.Album} - {request.Artist}");
        
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        bool sucesso = await _scrobbleService.ScrobbleAlbumAsync(request.Artist, request.Album, unixTimestamp, request.SessionKey);

        if (sucesso)
        {
            return Ok(new { mensagem = $"Álbum '{request.Album}' registrado com sucesso!" });
        }

        return BadRequest(new { mensagem = "Falha ao enviar scrobble para o Last.fm." });
    }
}