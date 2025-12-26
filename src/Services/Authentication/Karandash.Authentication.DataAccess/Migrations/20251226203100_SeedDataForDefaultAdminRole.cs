using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karandash.Authentication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForDefaultAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "InsertedAt", "IsVerified", "LastName", "PasswordHash", "PasswordSalt", "PendingEmail", "RefreshToken", "RefreshTokenExpireDate", "RemovedAt", "UpdatedAt", "UserRole" },
                values: new object[] { new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"), "info.karandashmmc@gmail.com", "System", new DateTime(2025, 12, 26, 20, 31, 0, 124, DateTimeKind.Utc).AddTicks(5854), true, "Administrator", "gmr+kg55gqEC0RJIlql4CKFHsB2uWTXubsgPiZLr/qU=", new byte[] { 234, 109, 79, 6, 196, 7, 0, 122, 243, 189, 176, 201, 98, 179, 241, 232 }, null, null, null, null, null, (byte)0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"));
        }
    }
}
