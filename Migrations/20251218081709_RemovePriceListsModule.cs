using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class RemovePriceListsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // price-lists modülünü sil (ayrı bir modül yok, fiyatlar Product entity'sinde)
            migrationBuilder.Sql(@"
                DELETE FROM ""CapabilityModules"" WHERE ""Code"" = 'price-lists';
            ");

            // Localization kayitlarini sil
            migrationBuilder.Sql(@"
                DELETE FROM ""LocaleStringResources"" WHERE ""ResourceName"" = 'Module.PriceLists';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NO-OP: CapabilityModules table is dropped in later migration (MigrateToGlobalPlatformModules)
            // The VendorCapabilities seed data doesn't exist at migration time.
        }
    }
}
