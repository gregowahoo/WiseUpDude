using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserQuizAttemptQuestionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestion");

            migrationBuilder.RenameColumn(
                name: "QuizQuestionId",
                table: "UserQuizAttemptQuestion",
                newName: "UserQuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizAttemptQuestion_QuizQuestionId",
                table: "UserQuizAttemptQuestion",
                newName: "IX_UserQuizAttemptQuestion_UserQuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizQuestions_UserQuizQuestionId",
                table: "UserQuizAttemptQuestion",
                column: "UserQuizQuestionId",
                principalTable: "UserQuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizQuestions_UserQuizQuestionId",
                table: "UserQuizAttemptQuestion");

            migrationBuilder.RenameColumn(
                name: "UserQuizQuestionId",
                table: "UserQuizAttemptQuestion",
                newName: "QuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizAttemptQuestion_UserQuizQuestionId",
                table: "UserQuizAttemptQuestion",
                newName: "IX_UserQuizAttemptQuestion_QuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestion",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
