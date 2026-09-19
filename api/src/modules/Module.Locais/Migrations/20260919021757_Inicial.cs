using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Locais.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Locais");

            migrationBuilder.CreateTable(
                name: "Locais",
                schema: "Locais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LocalDescricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EnderecoLogradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EnderecoNumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EnderecoBairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EnderecoCidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnderecoUf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    EnderecoCep = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
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
                    table.PrimaryKey("PK_Locais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Locais",
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
                name: "Salas",
                schema: "Locais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaNome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SalaCapacidade = table.Column<int>(type: "integer", nullable: false),
                    SalaTipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SalaRecursos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Salas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Salas_Locais_LocalId",
                        column: x => x.LocalId,
                        principalSchema: "Locais",
                        principalTable: "Locais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locais_ExcluidoEm",
                schema: "Locais",
                table: "Locais",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Locais_LocalNome",
                schema: "Locais",
                table: "Locais",
                column: "LocalNome",
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pendentes",
                schema: "Locais",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "LockedUntil", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Salas_ExcluidoEm",
                schema: "Locais",
                table: "Salas",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Salas_LocalId_SalaNome",
                schema: "Locais",
                table: "Salas",
                columns: new[] { "LocalId", "SalaNome" },
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Locais");

            migrationBuilder.DropTable(
                name: "Salas",
                schema: "Locais");

            migrationBuilder.DropTable(
                name: "Locais",
                schema: "Locais");
        }
    }
}
