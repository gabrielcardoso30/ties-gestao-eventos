using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Palestras.Migrations
{
    /// <inheritdoc />
    public partial class VincularTrilha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrilhaId",
                schema: "Palestras",
                table: "Palestras",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE "Palestras"."Palestras" p
                SET "TrilhaId" = (
                    SELECT tr."Id" FROM "Eventos"."Trilhas" tr
                    WHERE tr."EventoId" = p."EventoId" AND tr."ExcluidoEm" IS NULL
                    ORDER BY tr."CriadoEm" LIMIT 1)
                WHERE p."TrilhaId" = '00000000-0000-0000-0000-000000000000'::uuid;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Palestras_TrilhaId",
                schema: "Palestras",
                table: "Palestras",
                column: "TrilhaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Palestras_TrilhaId",
                schema: "Palestras",
                table: "Palestras");

            migrationBuilder.DropColumn(
                name: "TrilhaId",
                schema: "Palestras",
                table: "Palestras");
        }
    }
}
