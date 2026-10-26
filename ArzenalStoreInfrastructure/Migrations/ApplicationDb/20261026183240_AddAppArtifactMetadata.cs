using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arzenal.Store.Api.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddAppArtifactMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppArtifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AppId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Locator = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Version = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelativePath = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppArtifacts_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AppArtifacts_AppId_Type_Locator",
                table: "AppArtifacts",
                columns: new[] { "AppId", "Type", "Locator" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppArtifacts");
        }
    }
}
