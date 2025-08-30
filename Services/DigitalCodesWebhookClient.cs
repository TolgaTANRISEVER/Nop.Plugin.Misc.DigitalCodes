using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

public interface IDigitalCodesWebhookClient
{
    Task<(bool success, string response, int statusCode)> PostAsync(object payload, string url, string secret, int timeoutSeconds, int retryCount);
}

public class DigitalCodesWebhookClient : IDigitalCodesWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DigitalCodesWebhookClient> _logger;

    public DigitalCodesWebhookClient(HttpClient httpClient, ILogger<DigitalCodesWebhookClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(bool success, string response, int statusCode)> PostAsync(object payload, string url, string secret, int timeoutSeconds, int retryCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // HMAC-SHA256 imza (isteğe özel başlık)
        if (!string.IsNullOrEmpty(secret))
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
            var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();
            content.Headers.Add("X-DigitalCodes-Signature", signature);
        }

        var attempts = Math.Max(1, retryCount);
        for (var i = 1; i <= attempts; i++)
        {
            try
            {
                using var response = await _httpClient.PostAsync(url, content);
                var respText = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return (true, respText, (int)response.StatusCode);
                }
                _logger.LogWarning("Webhook çağrısı başarısız. Status={Status} Try={Try} Body={Body}", (int)response.StatusCode, i, respText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook çağrısı hata aldı. Try={Try}", i);
            }
            await Task.Delay(TimeSpan.FromSeconds(2 * i));
        }

        return (false, "", 0);
    }
}