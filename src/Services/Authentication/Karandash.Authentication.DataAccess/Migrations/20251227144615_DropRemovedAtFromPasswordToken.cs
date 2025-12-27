using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karandash.Authentication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DropRemovedAtFromPasswordToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "PasswordTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2025, 12, 27, 14, 46, 14, 712, DateTimeKind.Utc).AddTicks(7877));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "PasswordTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2025, 12, 26, 20, 31, 0, 124, DateTimeKind.Utc).AddTicks(5854));
        }
    }
}
