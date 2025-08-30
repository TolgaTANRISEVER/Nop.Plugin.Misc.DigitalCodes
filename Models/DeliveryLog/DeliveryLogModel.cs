using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models.DeliveryLog;

public record DeliveryLogModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.OrderId")]
    public int OrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.OrderItemId")]
    public int OrderItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.CodeItemId")]
    public int CodeItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Channel")]
    public string Channel { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.To")]
    public string To { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Result")]
    public string Result { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Message")]
    public string Message { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}