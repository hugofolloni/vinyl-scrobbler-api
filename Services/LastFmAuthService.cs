using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ScrobblerApi.Models.Config;

namespace ScrobblerApi.Services;

public class LastFmAuthService
{
    private readonly LastFmSettings _settings;

    public LastFmAuthService(IOptions<LastFmSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateApiSignature(Dictionary<string, string> parameters)
    {
        // A MÁGICA ESTÁ AQUI: StringComparer.Ordinal garante a ordem ASCII estrita!
        var sortedParams = parameters.OrderBy(p => p.Key, StringComparer.Ordinal);

        var signatureString = new StringBuilder();
        foreach (var param in sortedParams)
        {
            signatureString.Append(param.Key);
            signatureString.Append(param.Value);
        }

        signatureString.Append(_settings.SharedSecret);

        using (var md5 = MD5.Create())
        {
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signatureString.ToString()));
            var hashString = new StringBuilder();
            foreach (var b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }
            return hashString.ToString();
        }
    }
}