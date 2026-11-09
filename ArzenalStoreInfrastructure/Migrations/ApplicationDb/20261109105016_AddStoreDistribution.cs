using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arzenal.Store.Api.Infrastructure.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddStoreDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Changelog",
                table: "Releases",
                type: "varchar(16000)",
                maxLength: 16000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Releases",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Apps",
                type: "varchar(160)",
                maxLength: 160,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "AndroidVersionCode",
                table: "AppArtifacts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Architecture",
                table: "AppArtifacts",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MinimumOsVersion",
                table: "AppArtifacts",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PackageId",
                table: "AppArtifacts",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PackageType",
                table: "AppArtifacts",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PackageVersion",
                table: "AppArtifacts",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublisherKeyId",
                table: "AppArtifacts",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublisherSignature",
                table: "AppArtifacts",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "AppArtifacts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "AppArtifacts",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Releases_Id_AppId",
                table: "Releases",
                columns: new[] { "Id", "AppId" });

            migrationBuilder.CreateTable(
                name: "PublicationAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AppId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReleaseId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ArtifactId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ActorId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationAudits", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_Slug",
                table: "Apps",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppArtifacts_ReleaseId_AppId",
                table: "AppArtifacts",
                columns: new[] { "ReleaseId", "AppId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppArtifacts_ReleaseId_Platform_Architecture_PackageType",
                table: "AppArtifacts",
                columns: new[] { "ReleaseId", "Platform", "Architecture", "PackageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationAudits_AppId_OccurredAt",
                table: "PublicationAudits",
                columns: new[] { "AppId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationAudits_ReleaseId_OccurredAt",
                table: "PublicationAudits",
                columns: new[] { "ReleaseId", "OccurredAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppArtifacts_Releases_ReleaseId_AppId",
                table: "AppArtifacts",
                columns: new[] { "ReleaseId", "AppId" },
                principalTable: "Releases",
                principalColumns: new[] { "Id", "AppId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("UPDATE Releases SET Channel = 'stable' WHERE Channel = '';");
            // Les anciens binaires restent privés. Leur rattachement ne les valide ni ne les publie.
            migrationBuilder.Sql("""
                INSERT INTO Releases (Id, AppId, Version, Status, Channel, CreatedAt, PublishedAt, Changelog)
                SELECT UUID(), files.AppId, files.Version, 'Draft', 'stable', MIN(files.UploadedAt), NULL, NULL
                FROM AppArtifacts files
                WHERE files.Type = 'app' AND files.Version IS NOT NULL AND CHAR_LENGTH(files.Version) <= 50
                  AND NOT EXISTS (SELECT 1 FROM Releases r WHERE r.AppId = files.AppId AND r.Version = files.Version)
                GROUP BY files.AppId, files.Version;
                """);
            migrationBuilder.Sql("""
                UPDATE AppArtifacts files JOIN Releases r ON r.AppId = files.AppId AND r.Version = files.Version
                SET files.ReleaseId = r.Id WHERE files.Type = 'app' AND files.ReleaseId IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppArtifacts_Releases_ReleaseId_AppId",
                table: "AppArtifacts");

            migrationBuilder.DropTable(
                name: "PublicationAudits");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Releases_Id_AppId",
                table: "Releases");

            migrationBuilder.DropIndex(
                name: "IX_Apps_Slug",
                table: "Apps");

            migrationBuilder.DropIndex(
                name: "IX_AppArtifacts_ReleaseId_AppId",
                table: "AppArtifacts");

            migrationBuilder.DropIndex(
                name: "IX_AppArtifacts_ReleaseId_Platform_Architecture_PackageType",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "Changelog",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Apps");

            migrationBuilder.DropColumn(
                name: "AndroidVersionCode",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "Architecture",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "MinimumOsVersion",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "PackageVersion",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "PublisherKeyId",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "PublisherSignature",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "AppArtifacts");

            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "AppArtifacts");
        }
    }
}
