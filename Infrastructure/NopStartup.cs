using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Plugin.Misc.DigitalCodes.Services;

namespace Nop.Plugin.Misc.DigitalCodes.Infrastructure;

/// <summary>
/// Uygulama başlatılırken servisleri yapılandırır
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // HttpClient (proxy desteği ile)
        services.AddHttpClient<DigitalCodesWebhookClient>().WithProxy();

        // servis kayıtları
        services.AddScoped<IDigitalCodesWebhookClient, DigitalCodesWebhookClient>();
        services.AddScoped<DigitalCodesDeliveryService>();
        services.AddScoped<ICodePoolService, CodePoolService>();
        services.AddScoped<ICodeItemService, CodeItemService>();
        services.AddScoped<IDeliveryLogService, DeliveryLogService>();
        // Consumers, IConsumer<> implementasyonu ile otomatik resolve edilir.
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}