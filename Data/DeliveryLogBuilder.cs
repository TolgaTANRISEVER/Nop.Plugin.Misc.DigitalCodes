using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Data;

public class DeliveryLogBuilder : NopEntityBuilder<DeliveryLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(DeliveryLog.OrderId)).AsInt32().NotNullable().ForeignKey<Order>()
            .WithColumn(nameof(DeliveryLog.OrderItemId)).AsInt32().NotNullable().ForeignKey<OrderItem>()
            .WithColumn(nameof(DeliveryLog.CodeItemId)).AsInt32().NotNullable().ForeignKey<CodeItem>()
            .WithColumn(nameof(DeliveryLog.Channel)).AsString(50).NotNullable()
            .WithColumn(nameof(DeliveryLog.To)).AsString(200).Nullable()
            .WithColumn(nameof(DeliveryLog.Result)).AsString(200).Nullable()
            .WithColumn(nameof(DeliveryLog.Message)).AsString(1000).Nullable()
            .WithColumn(nameof(DeliveryLog.CreatedOnUtc)).AsDateTime().NotNullable();
    }
}