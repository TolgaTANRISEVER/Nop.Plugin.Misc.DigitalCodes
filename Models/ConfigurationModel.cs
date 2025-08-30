using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models;

public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.EnableWebhook")]
    public bool EnableWebhook { get; set; }
    public bool EnableWebhook_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.WebhookUrl")]
    public string WebhookUrl { get; set; }
    public bool WebhookUrl_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Secret")]
    public string Secret { get; set; }
    public bool Secret_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.TimeoutSeconds")]
    public int TimeoutSeconds { get; set; }
    public bool TimeoutSeconds_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.RetryCount")]
    public int RetryCount { get; set; }
    public bool RetryCount_OverrideForStore { get; set; }
}