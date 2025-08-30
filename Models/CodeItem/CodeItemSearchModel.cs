using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models.CodeItem;

public record CodeItemSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.CodePool")]
    public int? SearchCodePoolId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Status")]
    public int? SearchStatus { get; set; }

    public int? SearchOrderItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Code")]
    public string SearchCode { get; set; }
}