using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class CleanupNonStateCountries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sadece gerçek "State" sistemine sahip 13 ülkenin state'lerini tut
            // Digerlerini sil (StateTranslations önce silinmeli - FK constraint)

            // 13 ülke: US, DE, AU, AT, BR, IN, MX, MY, MM, NG, PW, SS, FM
            var countriesWithStates = "'US', 'DE', 'AU', 'AT', 'BR', 'IN', 'MX', 'MY', 'MM', 'NG', 'PW', 'SS', 'FM'";

            // Önce StateTranslations tablosundan sil (FK constraint)
            migrationBuilder.Sql($@"
                DELETE FROM ""StateTranslations""
                WHERE ""StateId"" IN (
                    SELECT s.""Id"" FROM ""States"" s
                    INNER JOIN ""Countries"" c ON s.""CountryId"" = c.""Id""
                    WHERE c.""Iso2Code"" NOT IN ({countriesWithStates})
                )
            ");

            // Sonra States tablosundan sil
            migrationBuilder.Sql($@"
                DELETE FROM ""States""
                WHERE ""CountryId"" IN (
                    SELECT ""Id"" FROM ""Countries""
                    WHERE ""Iso2Code"" NOT IN ({countriesWithStates})
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
