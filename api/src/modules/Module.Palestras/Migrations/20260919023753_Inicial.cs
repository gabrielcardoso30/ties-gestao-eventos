using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Module.Palestras.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Palestras");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Palestras",
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
                name: "Palestras",
                schema: "Palestras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaId = table.Column<Guid>(type: "uuid", nullable: true),
                    PalestraTitulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PalestraDescricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PalestraInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PalestraFim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Palestras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                schema: "Palestras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PalestraId = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificadoCodigo = table.Column<string>(type: "character(12)", fixedLength: true, maxLength: 12, nullable: false),
                    CertificadoEmitidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CertificadoCargaHorariaMinutos = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Certificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificados_Palestras_PalestraId",
                        column: x => x.PalestraId,
                        principalSchema: "Palestras",
                        principalTable: "Palestras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PalestraConteudos",
                schema: "Palestras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PalestraId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConteudoTitulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConteudoTipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConteudoUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ConteudoDescricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PalestraConteudos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalestraConteudos_Palestras_PalestraId",
                        column: x => x.PalestraId,
                        principalSchema: "Palestras",
                        principalTable: "Palestras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PalestraPalestrantes",
                schema: "Palestras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PalestraId = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PalestrantePapel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_PalestraPalestrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PalestraPalestrantes_Palestras_PalestraId",
                        column: x => x.PalestraId,
                        principalSchema: "Palestras",
                        principalTable: "Palestras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Presencas",
                schema: "Palestras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PalestraId = table.Column<Guid>(type: "uuid", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresencaRegistradaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Presencas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presencas_Palestras_PalestraId",
                        column: x => x.PalestraId,
                        principalSchema: "Palestras",
                        principalTable: "Palestras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CertificadoCodigo",
                schema: "Palestras",
                table: "Certificados",
                column: "CertificadoCodigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_ExcluidoEm",
                schema: "Palestras",
                table: "Certificados",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_PalestraId_PessoaId",
                schema: "Palestras",
                table: "Certificados",
                columns: new[] { "PalestraId", "PessoaId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pendentes",
                schema: "Palestras",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "LockedUntil", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_PalestraConteudos_ExcluidoEm",
                schema: "Palestras",
                table: "PalestraConteudos",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_PalestraConteudos_PalestraId",
                schema: "Palestras",
                table: "PalestraConteudos",
                column: "PalestraId");

            migrationBuilder.CreateIndex(
                name: "IX_PalestraPalestrantes_ExcluidoEm",
                schema: "Palestras",
                table: "PalestraPalestrantes",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_PalestraPalestrantes_PalestraId_PessoaId",
                schema: "Palestras",
                table: "PalestraPalestrantes",
                columns: new[] { "PalestraId", "PessoaId" },
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PalestraPalestrantes_PessoaId",
                schema: "Palestras",
                table: "PalestraPalestrantes",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Palestras_EventoId",
                schema: "Palestras",
                table: "Palestras",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Palestras_ExcluidoEm",
                schema: "Palestras",
                table: "Palestras",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Palestras_SalaId_PalestraInicio",
                schema: "Palestras",
                table: "Palestras",
                columns: new[] { "SalaId", "PalestraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Presencas_ExcluidoEm",
                schema: "Palestras",
                table: "Presencas",
                column: "ExcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Presencas_PalestraId_PessoaId",
                schema: "Palestras",
                table: "Presencas",
                columns: new[] { "PalestraId", "PessoaId" },
                unique: true,
                filter: "\"ExcluidoEm\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Presencas_PessoaId",
                schema: "Palestras",
                table: "Presencas",
                column: "PessoaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificados",
                schema: "Palestras");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Palestras");

            migrationBuilder.DropTable(
                name: "PalestraConteudos",
                schema: "Palestras");

            migrationBuilder.DropTable(
                name: "PalestraPalestrantes",
                schema: "Palestras");

            migrationBuilder.DropTable(
                name: "Presencas",
                schema: "Palestras");

            migrationBuilder.DropTable(
                name: "Palestras",
                schema: "Palestras");
        }
    }
}
