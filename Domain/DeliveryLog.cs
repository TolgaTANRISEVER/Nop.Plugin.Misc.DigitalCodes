using Nop.Core;
using System;

namespace Nop.Plugin.Misc.DigitalCodes.Domain;

public class DeliveryLog : BaseEntity
{
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int CodeItemId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, etc.
    public string? To { get; set; }
    public string? Result { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}