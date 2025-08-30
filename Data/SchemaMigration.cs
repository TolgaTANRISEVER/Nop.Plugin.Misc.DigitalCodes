using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Data;

[NopMigration("2025-08-22 00:00:00", "Misc.DigitalCodes base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        // Order is important due to FKs
        Create.TableFor<CodePool>();
        Create.TableFor<CodeItem>();
        Create.TableFor<DeliveryLog>();
    }
}