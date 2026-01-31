using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddMyJobsModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Get Service Providers section ID and insert My Jobs modules
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    v_parent_id INT;
                BEGIN
                    -- Get Service Providers section ID
                    SELECT ""Id"" INTO v_parent_id
                    FROM ""PlatformModules""
                    WHERE ""DisplayNameResourceKey"" = 'Dashboard.Section.ServiceProviders';

                    IF v_parent_id IS NOT NULL THEN
                        -- My Logistics Jobs
                        INSERT INTO ""PlatformModules"" (""ParentId"", ""Name"", ""DisplayName"", ""DisplayNameResourceKey"", ""Description"", ""Icon"", ""Route"", ""DisplayOrder"", ""IsMenuItem"", ""IsActive"", ""IsMenuSection"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                        VALUES (v_parent_id, 'MyLogisticsJobs', 'Lojistik Islerim', 'Module.MyLogisticsJobs', 'Kabul edilen lojistik isleri', 'bi-truck', '/Services/MyLogisticsJobs', 21, true, true, false, false, NOW(), NOW())
                        ON CONFLICT DO NOTHING;

                        -- My Customs Jobs
                        INSERT INTO ""PlatformModules"" (""ParentId"", ""Name"", ""DisplayName"", ""DisplayNameResourceKey"", ""Description"", ""Icon"", ""Route"", ""DisplayOrder"", ""IsMenuItem"", ""IsActive"", ""IsMenuSection"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                        VALUES (v_parent_id, 'MyCustomsJobs', 'Gumruk Islerim', 'Module.MyCustomsJobs', 'Kabul edilen gumruk isleri', 'bi-flag', '/Services/MyCustomsJobs', 22, true, true, false, false, NOW(), NOW())
                        ON CONFLICT DO NOTHING;

                        -- My Insurance Jobs
                        INSERT INTO ""PlatformModules"" (""ParentId"", ""Name"", ""DisplayName"", ""DisplayNameResourceKey"", ""Description"", ""Icon"", ""Route"", ""DisplayOrder"", ""IsMenuItem"", ""IsActive"", ""IsMenuSection"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                        VALUES (v_parent_id, 'MyInsuranceJobs', 'Sigorta Islerim', 'Module.MyInsuranceJobs', 'Kabul edilen sigorta isleri', 'bi-shield-check', '/Services/MyInsuranceJobs', 23, true, true, false, false, NOW(), NOW())
                        ON CONFLICT DO NOTHING;

                        -- My Survey Jobs
                        INSERT INTO ""PlatformModules"" (""ParentId"", ""Name"", ""DisplayName"", ""DisplayNameResourceKey"", ""Description"", ""Icon"", ""Route"", ""DisplayOrder"", ""IsMenuItem"", ""IsActive"", ""IsMenuSection"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                        VALUES (v_parent_id, 'MySurveyJobs', 'Gozetim Islerim', 'Module.MySurveyJobs', 'Kabul edilen gozetim isleri', 'bi-search', '/Services/MySurveyJobs', 24, true, true, false, false, NOW(), NOW())
                        ON CONFLICT DO NOTHING;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""PlatformModules""
                WHERE ""Name"" IN ('MyLogisticsJobs', 'MyCustomsJobs', 'MyInsuranceJobs', 'MySurveyJobs');
            ");
        }
    }
}
