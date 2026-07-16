using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;
using ScrobblerApi.Services;
using System.Text.Json;

namespace ScrobblerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LastFmSettings _settings;
    private readonly LastFmAuthService _authService;
    private readonly HttpClient _httpClient;

    public AuthController(IOptions<LastFmSettings> options, LastFmAuthService authService, HttpClient httpClient)
    {
        _settings = options.Value;
        _authService = authService;
        _httpClient = httpClient;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        // Deixa o .NET decidir se é HTTP ou HTTPS automaticamente
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/callback";
        var lastFmAuthUrl = $"http://www.last.fm/api/auth/?api_key={_settings.ApiKey}&cb={callbackUrl}";
        
        return Redirect(lastFmAuthUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token)) return BadRequest("Nenhum token recebido do Last.fm.");

        var parameters = new Dictionary<string, string>
        {
            { "api_key", _settings.ApiKey },
            { "method", "auth.getSession" },
            { "token", token }
        };

        var apiSig = _authService.GenerateApiSignature(parameters);

        var requestUrl = $"{_settings.BaseUrl}?method=auth.getSession&api_key={_settings.ApiKey}&token={token}&api_sig={apiSig}&format=json";

        var response = await _httpClient.GetAsync(requestUrl);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var jsonDoc = JsonDocument.Parse(content);
            var session = jsonDoc.RootElement.GetProperty("session");
            var sk = session.GetProperty("key").GetString();
            var name = session.GetProperty("name").GetString();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n========================================");
            Console.WriteLine($"[LAST.FM AUTH SUCESSO] Usuário: {name}");
            Console.WriteLine($"[SESSION KEY]: {sk}");
            Console.WriteLine("========================================\n");
            Console.ResetColor();

            return Ok(new { 
                mensagem = "Tudo certo! Pode fechar essa aba.", 
                usuario = name, 
                sessionKey = sk 
            });
        }

        return BadRequest($"Erro ao conectar com Last.fm: {content}");
    }

    // =========================================================================
    // 🆕 ADIÇÃO: FLUXO DE AUTENTICAÇÃO WEB (Isolado para o React)
    // =========================================================================

    [HttpGet("web/login")]
    public IActionResult WebLogin()
    {
        // Tem de ter o /web/callback aqui!
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/web/callback";
        var lastFmAuthUrl = $"http://www.last.fm/api/auth/?api_key={_settings.ApiKey}&cb={callbackUrl}";
        
        return Redirect(lastFmAuthUrl);
    }

    [HttpGet("web/callback")]
    public async Task<IActionResult> WebCallback([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token)) return BadRequest("Nenhum token recebido do Last.fm.");

        var parameters = new Dictionary<string, string>
        {
            { "api_key", _settings.ApiKey },
            { "method", "auth.getSession" },
            { "token", token }
        };

        var apiSig = _authService.GenerateApiSignature(parameters);
        var requestUrl = $"{_settings.BaseUrl}?method=auth.getSession&api_key={_settings.ApiKey}&token={token}&api_sig={apiSig}&format=json";

        var response = await _httpClient.GetAsync(requestUrl);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var jsonDoc = JsonDocument.Parse(content);
            var session = jsonDoc.RootElement.GetProperty("session");
            var sk = session.GetProperty("key").GetString();
            var name = session.GetProperty("name").GetString();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[LAST.FM AUTH WEB] React logado com sucesso! Usuário: {name}");
            Console.ResetColor();

            // Redireciona de volta para o frontend React (porta padrão do Vite)
            var reactRedirectUrl = $"http://localhost:5173/?sk={sk}&username={name}";
            return Redirect(reactRedirectUrl);
        }

        return BadRequest($"Erro na autenticação Web: {content}");
    }
}