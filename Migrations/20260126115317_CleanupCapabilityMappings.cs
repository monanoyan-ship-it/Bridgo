using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class CleanupCapabilityMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Yanlis eklenen CapabilityModuleMappings kayitlarini sil
            migrationBuilder.Sql(@"
                DELETE FROM ""CapabilityModuleMappings""
                WHERE ""PlatformModuleId"" IN (
                    SELECT ""Id"" FROM ""PlatformModules""
                    WHERE ""Name"" IN ('MyLogisticsJobs', 'MyCustomsJobs', 'MyInsuranceJobs', 'MySurveyJobs')
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
