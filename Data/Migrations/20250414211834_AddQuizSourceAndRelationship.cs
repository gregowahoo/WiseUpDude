using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizSourceAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizSourceId",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuizSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizSourceId",
                table: "Quizzes",
                column: "QuizSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizSources_QuizSourceId",
                table: "Quizzes",
                column: "QuizSourceId",
                principalTable: "QuizSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizSources_QuizSourceId",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizSources");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_QuizSourceId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "QuizSourceId",
                table: "Quizzes");
        }
    }
}
