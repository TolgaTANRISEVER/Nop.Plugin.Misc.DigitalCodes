using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models.CodePool;

public record CodePoolSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.PoolName")]
    public string SearchName { get; set; }

    public int? SearchProductId { get; set; }
}