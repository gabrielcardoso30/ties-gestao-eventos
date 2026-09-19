using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Eventos.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTrilhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trilhas",
                schema: "Eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrilhaNome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TrilhaDescricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TrilhaCor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AlteradoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AlteradoPor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ExcluidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExcluidoPor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trilhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trilhas_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalSchema: "Eventos",
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trilhas_EventoId_TrilhaNome",
                schema: "Eventos",
                table: "Trilhas",
                columns: new[] { "EventoId", "TrilhaNome" },
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Trilhas_ExcluidoEm",
                schema: "Eventos",
                table: "Trilhas",
                column: "ExcluidoEm");

            // Compatibilidade com eventos já existentes. A expressão mantém versão/variante de UUID v7.
            migrationBuilder.Sql("""
                INSERT INTO "Eventos"."Trilhas"
                    ("Id", "EventoId", "TrilhaNome", "TrilhaDescricao", "TrilhaCor", "CriadoEm", "CriadoPor", "EstaAtivo")
                SELECT
                    (lpad(to_hex((extract(epoch from clock_timestamp()) * 1000)::bigint), 12, '0') ||
                     '7' || substr(md5(random()::text), 2, 3) || '8' || substr(md5(random()::text), 1, 3) ||
                     substr(md5(random()::text), 1, 12))::uuid,
                    e."Id", 'Trilha única', NULL, '#2563EB', now(), 'migration', TRUE
                FROM "Eventos"."Eventos" e
                WHERE e."ExcluidoEm" IS NULL
                  AND NOT EXISTS (SELECT 1 FROM "Eventos"."Trilhas" t WHERE t."EventoId" = e."Id" AND t."ExcluidoEm" IS NULL);

                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trilhas",
                schema: "Eventos");
        }
    }
}
