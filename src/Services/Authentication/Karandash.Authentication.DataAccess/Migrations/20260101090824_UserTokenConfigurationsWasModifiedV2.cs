using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karandash.Authentication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserTokenConfigurationsWasModifiedV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2026, 1, 1, 9, 8, 24, 244, DateTimeKind.Utc).AddTicks(4792));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2026, 1, 1, 9, 3, 18, 227, DateTimeKind.Utc).AddTicks(5178));
        }
    }
}
