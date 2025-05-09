using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQuizAttemptEntities_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizAttemptId",
                principalTable: "UserQuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizAttemptId",
                principalTable: "UserQuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
