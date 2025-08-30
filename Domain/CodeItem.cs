using Nop.Core;
using System;

namespace Nop.Plugin.Misc.DigitalCodes.Domain;

public enum CodeItemStatus
{
    Available = 0,
    Reserved = 1,
    Delivered = 2,
    Disabled = 3
}

public class CodeItem : BaseEntity
{
    public int CodePoolId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Pin { get; set; }
    public string? Serial { get; set; }
    public DateTime? ExpireOnUtc { get; set; }
    public int Status { get; set; }
    public int? OrderItemId { get; set; }
    public DateTime? ReservedUntilUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}