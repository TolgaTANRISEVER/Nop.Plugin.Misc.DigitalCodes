using Nop.Web.Framework.Menu;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Plugin.Misc.DigitalCodes.Infrastructure;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

/// <summary>
/// Admin menüde eklenti bağlantısını eklemek için event consumer
/// </summary>
public class EventConsumer : BaseAdminMenuCreatedEventConsumer
{
    public EventConsumer(IPluginManager<IPlugin> pluginManager) : base(pluginManager)
    {
    }

    protected override string PluginSystemName => DigitalCodesDefaults.SystemName;

    protected override string BeforeMenuSystemName => "Local plugins";

    protected override Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
    {
        var descriptor = plugin.PluginDescriptor;

        var parent = new AdminMenuItem
        {
            Visible = true,
            SystemName = descriptor.SystemName,
            Title = descriptor.FriendlyName,
            IconClass = "far fa-dot-circle",
            ChildNodes = new List<AdminMenuItem>
            {
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}.Configuration",
                    Title = "Ayarlar",
                    Url = plugin.GetConfigurationPageUrl(),
                    IconClass = "far fa-circle"
                },
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}.CodePools",
                    Title = "Kod Havuzları",
                    Url = "/Admin/DigitalCodes/CodePools",
                    IconClass = "far fa-circle"
                },
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}.CodeItems",
                    Title = "Kod Kalemleri",
                    Url = "/Admin/DigitalCodes/CodeItems",
                    IconClass = "far fa-circle"
                },
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}.DeliveryLogs",
                    Title = "Teslimat Logları",
                    Url = "/Admin/DigitalCodes/DeliveryLogs",
                    IconClass = "far fa-circle"
                }
            }
        };

        return Task.FromResult(parent);
    }
}