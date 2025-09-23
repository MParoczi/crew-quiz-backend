using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_User_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "QuestionGroup",
                columns: table => new
                {
                    QuestionGroupId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionGroup", x => x.QuestionGroupId);
                    table.ForeignKey(
                        name: "FK_QuestionGroup_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionGroup_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Quiz",
                columns: table => new
                {
                    QuizId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz", x => x.QuizId);
                    table.ForeignKey(
                        name: "FK_Quiz_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quiz_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    QuestionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Inquiry = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Point = table.Column<short>(type: "smallint", nullable: false),
                    QuestionGroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_Question_QuestionGroup_QuestionGroupId",
                        column: x => x.QuestionGroupId,
                        principalTable: "QuestionGroup",
                        principalColumn: "QuestionGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CurrentGame",
                columns: table => new
                {
                    CurrentGameId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    QuizId = table.Column<long>(type: "bigint", nullable: false),
                    IsStarted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentGame", x => x.CurrentGameId);
                    table.ForeignKey(
                        name: "FK_CurrentGame_Quiz_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quiz",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGame_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGame_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "QuestionGroupQuiz",
                columns: table => new
                {
                    QuestionGroupId = table.Column<long>(type: "bigint", nullable: false),
                    QuizId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionGroupQuiz", x => new { x.QuestionGroupId, x.QuizId });
                    table.ForeignKey(
                        name: "FK_QuestionGroupQuiz_QuestionGroup_QuestionGroupId",
                        column: x => x.QuestionGroupId,
                        principalTable: "QuestionGroup",
                        principalColumn: "QuestionGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionGroupQuiz_Quiz_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quiz",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionGroupQuiz_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionGroupQuiz_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CurrentGameQuestion",
                columns: table => new
                {
                    CurrentGameId = table.Column<long>(type: "bigint", nullable: false),
                    QuestionId = table.Column<long>(type: "bigint", nullable: false),
                    IsAnswered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRobbingAllowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AnsweredByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentGameQuestion", x => new { x.CurrentGameId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_CurrentGame_CurrentGameId",
                        column: x => x.CurrentGameId,
                        principalTable: "CurrentGame",
                        principalColumn: "CurrentGameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_User_AnsweredByUserId",
                        column: x => x.AnsweredByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CurrentGameQuestion_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CurrentGameUser",
                columns: table => new
                {
                    CurrentGameId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsGameMaster = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentGameUser", x => new { x.CurrentGameId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CurrentGameUser_CurrentGame_CurrentGameId",
                        column: x => x.CurrentGameId,
                        principalTable: "CurrentGame",
                        principalColumn: "CurrentGameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGameUser_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrentGameUser_User_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CurrentGameUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGame_CreatedByUserId",
                table: "CurrentGame",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGame_QuizId",
                table: "CurrentGame",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGame_SessionId",
                table: "CurrentGame",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGame_UpdatedByUserId",
                table: "CurrentGame",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_AnsweredByUserId",
                table: "CurrentGameQuestion",
                column: "AnsweredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_CreatedByUserId",
                table: "CurrentGameQuestion",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_CurrentGameId",
                table: "CurrentGameQuestion",
                column: "CurrentGameId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_QuestionId",
                table: "CurrentGameQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_UpdatedByUserId",
                table: "CurrentGameQuestion",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameQuestion_UserId",
                table: "CurrentGameQuestion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameUser_CreatedByUserId",
                table: "CurrentGameUser",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameUser_CurrentGameId",
                table: "CurrentGameUser",
                column: "CurrentGameId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameUser_CurrentGameId_UserId",
                table: "CurrentGameUser",
                columns: new[] { "CurrentGameId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameUser_UpdatedByUserId",
                table: "CurrentGameUser",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentGameUser_UserId",
                table: "CurrentGameUser",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_CreatedByUserId",
                table: "Question",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionGroupId",
                table: "Question",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionGroupId_Inquiry",
                table: "Question",
                columns: new[] { "QuestionGroupId", "Inquiry" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_UpdatedByUserId",
                table: "Question",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_CreatedByUserId",
                table: "QuestionGroup",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_CreatedByUserId_Name",
                table: "QuestionGroup",
                columns: new[] { "CreatedByUserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroup_UpdatedByUserId",
                table: "QuestionGroup",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroupQuiz_CreatedByUserId",
                table: "QuestionGroupQuiz",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroupQuiz_QuestionGroupId",
                table: "QuestionGroupQuiz",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroupQuiz_QuestionGroupId_QuizId",
                table: "QuestionGroupQuiz",
                columns: new[] { "QuestionGroupId", "QuizId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroupQuiz_QuizId",
                table: "QuestionGroupQuiz",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionGroupQuiz_UpdatedByUserId",
                table: "QuestionGroupQuiz",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_CreatedByUserId",
                table: "Quiz",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_Name_CreatedByUserId",
                table: "Quiz",
                columns: new[] { "Name", "CreatedByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_UpdatedByUserId",
                table: "Quiz",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CreatedByUserId",
                table: "User",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UpdatedByUserId",
                table: "User",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentGameQuestion");

            migrationBuilder.DropTable(
                name: "CurrentGameUser");

            migrationBuilder.DropTable(
                name: "QuestionGroupQuiz");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "CurrentGame");

            migrationBuilder.DropTable(
                name: "QuestionGroup");

            migrationBuilder.DropTable(
                name: "Quiz");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
