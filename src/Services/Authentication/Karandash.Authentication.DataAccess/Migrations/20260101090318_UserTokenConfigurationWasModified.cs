using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karandash.Authentication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserTokenConfigurationWasModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordTokens_Users_UserId",
                table: "PasswordTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordTokens",
                table: "PasswordTokens");

            migrationBuilder.RenameTable(
                name: "PasswordTokens",
                newName: "UserTokens");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordTokens_UserId",
                table: "UserTokens",
                newName: "IX_UserTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2026, 1, 1, 9, 3, 18, 227, DateTimeKind.Utc).AddTicks(5178));

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                newName: "PasswordTokens");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_UserId",
                table: "PasswordTokens",
                newName: "IX_PasswordTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordTokens",
                table: "PasswordTokens",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("88b608c2-94a6-4c7d-9d55-b128018cab4e"),
                column: "InsertedAt",
                value: new DateTime(2026, 1, 1, 8, 59, 43, 832, DateTimeKind.Utc).AddTicks(8979));

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordTokens_Users_UserId",
                table: "PasswordTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
