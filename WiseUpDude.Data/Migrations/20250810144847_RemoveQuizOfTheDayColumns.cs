using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuizOfTheDayColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the index first if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Quizzes_QuizOfTheDay' AND object_id = OBJECT_ID('Quizzes'))
                BEGIN
                    DROP INDEX IX_Quizzes_QuizOfTheDay ON Quizzes;
                END");

            migrationBuilder.DropColumn(
                name: "IsQuizOfTheDay",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "QuizOfTheDayDate",
                table: "Quizzes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQuizOfTheDay",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuizOfTheDayDate",
                table: "Quizzes",
                type: "datetime2",
                nullable: true);

            // Recreate the index if rolling back
            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizOfTheDay",
                table: "Quizzes",
                column: "IsQuizOfTheDay");
        }
    }
}
