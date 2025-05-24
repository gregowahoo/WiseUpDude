using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserQuizQuestionNavigationFromUserQuizAttemptQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint with the correct name
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions");

            // Drop the foreign key constraint to UserQuizQuestion with the correct name
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizQuestions_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions");

            // Drop the index - this should be the correct name based on the constraint
            migrationBuilder.DropIndex(
                name: "IX_UserQuizAttemptQuestion_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions");

            // Add back the UserQuizAttempt relationship with Cascade delete
            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizAttemptId",
                principalTable: "UserQuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint with correct name
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions");

            // Recreate the index with correct name
            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttemptQuestion_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizQuestionId");

            // Recreate both foreign keys with correct names
            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizAttempt_UserQuizAttemptId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizAttemptId",
                principalTable: "UserQuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAttemptQuestion_UserQuizQuestions_UserQuizQuestionId",
                table: "UserQuizAttemptQuestions",
                column: "UserQuizQuestionId",
                principalTable: "UserQuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
