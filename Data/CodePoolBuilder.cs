using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Data;

public class CodePoolBuilder : NopEntityBuilder<CodePool>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CodePool.Name)).AsString(200).NotNullable()
            .WithColumn(nameof(CodePool.ProductId)).AsInt32().Nullable().ForeignKey<Product>()
            .WithColumn(nameof(CodePool.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn(nameof(CodePool.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(CodePool.UpdatedOnUtc)).AsDateTime().Nullable();
    }
}