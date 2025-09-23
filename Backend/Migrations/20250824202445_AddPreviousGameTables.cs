using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousGameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreviousGame",
                columns: table => new
                {
                    PreviousGameId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    QuizName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousGame", x => x.PreviousGameId);
                    table.ForeignKey(
                        name: "FK_PreviousGame_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreviousGame_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PreviousGameUser",
                columns: table => new
                {
                    PreviousGameId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGameMaster = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousGameUser", x => new { x.PreviousGameId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PreviousGameUser_PreviousGame_PreviousGameId",
                        column: x => x.PreviousGameId,
                        principalTable: "PreviousGame",
                        principalColumn: "PreviousGameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreviousGameUser_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreviousGameUser_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PreviousGameUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGame_CompletedOn",
                table: "PreviousGame",
                column: "CompletedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGame_CreatedByUserId",
                table: "PreviousGame",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGame_SessionId",
                table: "PreviousGame",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGame_UpdatedByUserId",
                table: "PreviousGame",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_CreatedByUserId",
                table: "PreviousGameUser",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_PreviousGameId",
                table: "PreviousGameUser",
                column: "PreviousGameId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_PreviousGameId_UserId",
                table: "PreviousGameUser",
                columns: new[] { "PreviousGameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_Rank",
                table: "PreviousGameUser",
                column: "Rank");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_UpdatedByUserId",
                table: "PreviousGameUser",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousGameUser_UserId",
                table: "PreviousGameUser",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreviousGameUser");

            migrationBuilder.DropTable(
                name: "PreviousGame");
        }
    }
}
