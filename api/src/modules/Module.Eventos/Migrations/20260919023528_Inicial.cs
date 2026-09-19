using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Eventos.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Eventos");

            migrationBuilder.CreateTable(
                name: "Eventos",
                schema: "Eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventoDescricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EventoDataInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventoDataFim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventoFormato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocalId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventoLinkRemoto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EventoSituacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventoCapacidadeMaxima = table.Column<int>(type: "integer", nullable: true),
                    EventoCancelamentoMotivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Eventos",
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
                name: "Inscricoes",
                schema: "Eventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    InscricaoSituacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InscricaoRealizadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InscricaoCanceladaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Inscricoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalSchema: "Eventos",
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_EventoDataInicio",
                schema: "Eventos",
                table: "Eventos",
                column: "EventoDataInicio");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_EventoSituacao",
                schema: "Eventos",
                table: "Eventos",
                column: "EventoSituacao");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_ExcluidoEm",
                schema: "Eventos",
                table: "Eventos",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_LocalId",
                schema: "Eventos",
                table: "Eventos",
                column: "LocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_EventoId_InscricaoSituacao",
                schema: "Eventos",
                table: "Inscricoes",
                columns: new[] { "EventoId", "InscricaoSituacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_EventoId_PessoaId_Confirmada",
                schema: "Eventos",
                table: "Inscricoes",
                columns: new[] { "EventoId", "PessoaId" },
                unique: true,
                filter: "\"InscricaoSituacao\" = 'Confirmada'");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_ExcluidoEm",
                schema: "Eventos",
                table: "Inscricoes",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pendentes",
                schema: "Eventos",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "LockedUntil", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inscricoes",
                schema: "Eventos");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Eventos");

            migrationBuilder.DropTable(
                name: "Eventos",
                schema: "Eventos");
        }
    }
}
