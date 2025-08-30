using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.DigitalCodes.Models.CodePool;

public record CodePoolModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.PoolName")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Product")]
    public int? ProductId { get; set; }

    public string ProductName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.IsActive")]
    public bool IsActive { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}