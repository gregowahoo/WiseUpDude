using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Migrations
{
    /// <inheritdoc />
    public partial class SomeUpdateNoIdea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "QuizSources",
                newName: "Topic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "QuizSources",
                newName: "Name");
        }
    }
}
