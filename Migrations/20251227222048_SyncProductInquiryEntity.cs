using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class SyncProductInquiryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Entity ProductInquiry veritabani ile senkronize edildi.
            // Bu degisiklikler zaten EnhanceProductInquiryAndTeamMember migration'inda yapilmisti.
            // Bu migration sadece model snapshot'i guncellemek icindir.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: Bu migration sadece model snapshot senkronizasyonu icindir.
        }
    }
}
