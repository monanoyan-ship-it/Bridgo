using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bridgo.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleToExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. User rolunu olustur (yoksa)
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""Name"", ""NormalizedName"", ""Description"", ""ConcurrencyStamp"", ""CreatedAt"")
                SELECT 'User', 'USER', 'Kayitli kullanici', gen_random_uuid()::text, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""Name"" = 'User');
            ");

            // 2. Tum mevcut kullanicilara User rolunu ata (henuz atanmamislara)
            migrationBuilder.Sql(@"
                INSERT INTO ""UserRoles"" (""UserId"", ""RoleId"")
                SELECT u.""Id"", r.""Id""
                FROM ""Users"" u
                CROSS JOIN ""Roles"" r
                WHERE r.""Name"" = 'User'
                AND NOT EXISTS (
                    SELECT 1 FROM ""UserRoles"" ur
                    WHERE ur.""UserId"" = u.""Id"" AND ur.""RoleId"" = r.""Id""
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // User rolunu kullanicilardan kaldir
            migrationBuilder.Sql(@"
                DELETE FROM ""UserRoles""
                WHERE ""RoleId"" = (SELECT ""Id"" FROM ""Roles"" WHERE ""Name"" = 'User');
            ");

            // User rolunu sil
            migrationBuilder.Sql(@"
                DELETE FROM ""Roles"" WHERE ""Name"" = 'User';
            ");
        }
    }
}
