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
                table: "UserQuizAttemptQuestions");

            migrationBuilder.RenameColumn(
                name: "QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                newName: "UserQuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizAttemptQuestion_QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                newName: "IX_UserQuizAttemptQuestion_UserQuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizQuestions_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions",
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
                table: "UserQuizAttemptQuestions");

            migrationBuilder.RenameColumn(
                name: "UserQuizQuestionId",
                table: "UserQuizAttemptQuestions",
                newName: "QuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_UserQuizAttemptQuestion_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions",
                newName: "IX_UserQuizAttemptQuestion_QuizQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_QuizQuestions_QuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
