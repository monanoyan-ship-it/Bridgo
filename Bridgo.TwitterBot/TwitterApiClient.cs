using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bridgo.TwitterBot;

/// <summary>
/// Twitter API v2 - OAuth 1.0a ile tweet atar.
/// Hicbir 3rd party kutuphane yok, saf HttpClient.
/// </summary>
public class TwitterApiClient
{
    private readonly TwitterConfig _config;
    private readonly HttpClient _http;
    private readonly ILogger<TwitterApiClient> _logger;

    public TwitterApiClient(TwitterConfig config, HttpClient http, ILogger<TwitterApiClient> logger)
    {
        _config = config;
        _http = http;
        _logger = logger;
    }

    public async Task<string?> PostTweetAsync(string text)
    {
        var url = "https://api.twitter.com/2/tweets";
        var body = JsonSerializer.Serialize(new { text });

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        var authHeader = BuildOAuthHeader("POST", url);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader);

        var response = await _http.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Twitter API hata: {Status} - {Body}", response.StatusCode, responseBody);
            return null;
        }

        // Response: {"data":{"id":"123","text":"..."}}
        using var doc = JsonDocument.Parse(responseBody);
        var tweetId = doc.RootElement.GetProperty("data").GetProperty("id").GetString();
        return tweetId;
    }

    private string BuildOAuthHeader(string method, string url)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString("N");

        var parameters = new SortedDictionary<string, string>
        {
            ["oauth_consumer_key"] = _config.ApiKey,
            ["oauth_nonce"] = nonce,
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = timestamp,
            ["oauth_token"] = _config.AccessToken,
            ["oauth_version"] = "1.0"
        };

        // Signature base string
        var paramString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        var baseString = $"{method}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";

        // Signing key
        var signingKey = $"{Uri.EscapeDataString(_config.ApiSecret)}&{Uri.EscapeDataString(_config.AccessTokenSecret)}";
        var signature = Convert.ToBase64String(HMACSHA1.HashData(Encoding.ASCII.GetBytes(signingKey), Encoding.ASCII.GetBytes(baseString)));

        parameters["oauth_signature"] = signature;

        return string.Join(", ", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}=\"{Uri.EscapeDataString(p.Value)}\""));
    }
}
