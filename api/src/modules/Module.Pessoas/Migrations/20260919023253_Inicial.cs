using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Pessoas.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Pessoas");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", maxLength: 200, nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TraceParent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pessoas",
                schema: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PessoaEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PessoaTelefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PessoaDocumento = table.Column<string>(type: "character(11)", fixedLength: true, maxLength: 11, nullable: true),
                    PessoaEmpresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PessoaCargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PessoaMiniBio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PessoaFotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pendentes",
                schema: "Pessoas",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "LockedUntil", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_ExcluidoEm",
                schema: "Pessoas",
                table: "Pessoas",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_PessoaDocumento",
                schema: "Pessoas",
                table: "Pessoas",
                column: "PessoaDocumento",
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL AND \"PessoaDocumento\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_PessoaEmail",
                schema: "Pessoas",
                table: "Pessoas",
                column: "PessoaEmail",
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_PessoaNome",
                schema: "Pessoas",
                table: "Pessoas",
                column: "PessoaNome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Pessoas");

            migrationBuilder.DropTable(
                name: "Pessoas",
                schema: "Pessoas");
        }
    }
}
