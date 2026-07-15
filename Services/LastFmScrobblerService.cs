using System.Security.Cryptography;
using System.Text;

namespace ScrobblerApi.Services;

public interface IScrobbleService
{
    Task<bool> ScrobbleAsync(string vinylName, string groupName);
}

public class LastFmScrobbleService : IScrobbleService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public LastFmScrobbleService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<bool> ScrobbleAsync(string vinylName, string groupName)
    {
        // No futuro, essas chaves virão do banco de dados e do appsettings.json
        string apiKey = _config["LastFm:ApiKey"] ?? "SUA_API_KEY_AQUI";
        string apiSecret = _config["LastFm:SharedSecret"] ?? "SEU_SECRET_AQUI";
        string sessionKey = _config["LastFm:SessionKey"] ?? "SESSION_KEY_DO_USUARIO_AQUI"; 

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string method = "track.scrobble";

        // 1. O Last.fm exige os parâmetros ordenados alfabeticamente para gerar a assinatura
        var parameters = new SortedDictionary<string, string>
        {
            { "api_key", apiKey },
            { "artist", groupName },
            { "method", method },
            { "sk", sessionKey },
            { "timestamp", timestamp },
            { "track", vinylName }
        };

        // 2. Concatena tudo para criar a assinatura
        var sigBuilder = new StringBuilder();
        foreach (var param in parameters)
        {
            sigBuilder.Append(param.Key).Append(param.Value);
        }
        sigBuilder.Append(apiSecret);

        string apiSig = GenerateMD5(sigBuilder.ToString());
        parameters.Add("api_sig", apiSig);
        parameters.Add("format", "json"); // Queremos a resposta em JSON, não em XML

        // 3. Envia o POST para a API
        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync("http://ws.audioscrobbler.com/2.0/", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorOutput = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[LastFm Error] {errorOutput}");
            return false;
        }

        return true;
    }

    private static string GenerateMD5(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}