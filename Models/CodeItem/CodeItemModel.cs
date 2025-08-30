using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.DigitalCodes.Models.CodeItem;

public record CodeItemModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.CodePool")]
    public int CodePoolId { get; set; }
    public string CodePoolName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Code")]
    [Required]
    public string Code { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Pin")]
    public string Pin { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Serial")]
    public string Serial { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.ExpireOnUtc")]
    public DateTime? ExpireOnUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.Status")]
    public int Status { get; set; }
    public string StatusText { get; set; }

    public int? OrderItemId { get; set; }
    public string OrderInfo { get; set; }

    [NopResourceDisplayName("Plugins.Misc.DigitalCodes.Fields.ReservedUntilUtc")]
    public DateTime? ReservedUntilUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}