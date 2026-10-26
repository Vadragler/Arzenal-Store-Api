using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arzenal.Store.Api.Infrastructure.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AbsoluteRefreshTokenExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AbsoluteExpiresAt",
                table: "RefreshTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsoluteExpiresAt",
                table: "RefreshTokens");
        }
    }
}
