using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.DigitalCodes;

/// <summary>
/// Dijital Kodlar (E-Pin) eklentisi için ayarlar
/// </summary>
public class DigitalCodesSettings : ISettings
{
    /// <summary>
    /// Webhook ile teslimatı etkinleştir
    /// </summary>
    public bool EnableWebhook { get; set; }

    /// <summary>
    /// Webhook POST isteklerinin gönderileceği URL
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    /// HMAC imzası için paylaşılan gizli anahtar
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Webhook HTTP isteği zaman aşımı (saniye)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// Başarısız webhook çağrısı için maksimum tekrar sayısı
    /// </summary>
    public int RetryCount { get; set; } = 3;
}