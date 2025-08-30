using Nop.Core;
using System;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.DigitalCodes.Domain;

public class CodePool : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}