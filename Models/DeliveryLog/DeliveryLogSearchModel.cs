using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models.DeliveryLog;

public record DeliveryLogSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.OrderId")]
    public int? SearchOrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.OrderItemId")]
    public int? SearchOrderItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.CodeItemId")]
    public int? SearchCodeItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Channel")]
    public string SearchChannel { get; set; }
}