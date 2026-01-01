using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karandash.Authentication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserTokenEntityWasModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "TokenType",
                table: "PasswordTokens",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2026, 1, 1, 8, 59, 43, 832, DateTimeKind.Utc).AddTicks(8979));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenType",
                table: "PasswordTokens");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2025, 12, 27, 14, 46, 14, 712, DateTimeKind.Utc).AddTicks(7877));
        }
    }
}
