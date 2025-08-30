using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Data;

public class CodeItemBuilder : NopEntityBuilder<CodeItem>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CodeItem.CodePoolId)).AsInt32().NotNullable().ForeignKey<CodePool>()
            .WithColumn(nameof(CodeItem.Code)).AsString(512).NotNullable()
            .WithColumn(nameof(CodeItem.Pin)).AsString(256).Nullable()
            .WithColumn(nameof(CodeItem.Serial)).AsString(256).Nullable()
            .WithColumn(nameof(CodeItem.ExpireOnUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(CodeItem.Status)).AsInt32().NotNullable().WithDefaultValue((int)CodeItemStatus.Available)
            .WithColumn(nameof(CodeItem.OrderItemId)).AsInt32().Nullable().ForeignKey<OrderItem>()
            .WithColumn(nameof(CodeItem.ReservedUntilUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(CodeItem.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(CodeItem.UpdatedOnUtc)).AsDateTime().Nullable();
    }
}