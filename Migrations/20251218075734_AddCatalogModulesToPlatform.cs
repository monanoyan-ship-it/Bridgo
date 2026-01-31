using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogModulesToPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NO-OP: CapabilityModules table is dropped in later migration (MigrateToGlobalPlatformModules)
            // The VendorCapabilities seed data doesn't exist at migration time, causing NULL constraint violations.
            // This migration's data would be deleted anyway, so we skip it.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NO-OP: Up() is no-op, so Down() is also no-op
        }
    }
}
