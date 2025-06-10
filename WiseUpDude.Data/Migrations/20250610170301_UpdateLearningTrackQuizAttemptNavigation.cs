using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLearningTrackQuizAttemptNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningTrackQuizAttempts_LearningTracks_LearningTrackId",
                table: "LearningTrackQuizAttempts");

            migrationBuilder.RenameColumn(
                name: "LearningTrackId",
                table: "LearningTrackQuizAttempts",
                newName: "LearningTrackQuizId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningTrackQuizAttempts_LearningTrackId",
                table: "LearningTrackQuizAttempts",
                newName: "IX_LearningTrackQuizAttempts_LearningTrackQuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningTrackQuizAttempts_LearningTrackQuizzes_LearningTrackQuizId",
                table: "LearningTrackQuizAttempts",
                column: "LearningTrackQuizId",
                principalTable: "LearningTrackQuizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningTrackQuizAttempts_LearningTrackQuizzes_LearningTrackQuizId",
                table: "LearningTrackQuizAttempts");

            migrationBuilder.RenameColumn(
                name: "LearningTrackQuizId",
                table: "LearningTrackQuizAttempts",
                newName: "LearningTrackId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningTrackQuizAttempts_LearningTrackQuizId",
                table: "LearningTrackQuizAttempts",
                newName: "IX_LearningTrackQuizAttempts_LearningTrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningTrackQuizAttempts_LearningTracks_LearningTrackId",
                table: "LearningTrackQuizAttempts",
                column: "LearningTrackId",
                principalTable: "LearningTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
