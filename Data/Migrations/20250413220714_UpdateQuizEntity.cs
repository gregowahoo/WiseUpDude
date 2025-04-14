using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Options",
                table: "QuizQuestions",
                newName: "OptionsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OptionsJson",
                table: "QuizQuestions",
                newName: "Options");
        }
    }
}
