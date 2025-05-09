using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQuizAttemptEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuizId = table.Column<int>(type: "int", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAttempt_UserQuizzes_UserQuizId",
                        column: x => x.UserQuizId,
                        principalTable: "UserQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAttemptQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuizAttemptId = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionId = table.Column<int>(type: "int", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    TimeTakenSeconds = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAttemptQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                        column: x => x.UserQuizAttemptId,
                        principalTable: "UserQuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttempt_UserQuizId",
                table: "UserQuizAttempts",
                column: "UserQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttemptQuestion_QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttemptQuestion_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizAttemptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQuizAttemptQuestions");

            migrationBuilder.DropTable(
                name: "UserQuizAttempts");
        }
    }
}
