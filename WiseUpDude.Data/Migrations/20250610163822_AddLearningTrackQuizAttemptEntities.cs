using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningTrackQuizAttemptEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningTrackQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearningTrackId = table.Column<int>(type: "int", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackQuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackQuizAttempts_LearningTracks_LearningTrackId",
                        column: x => x.LearningTrackId,
                        principalTable: "LearningTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrackAttemptQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearningTrackAttemptId = table.Column<int>(type: "int", nullable: false),
                    LearningTrackQuestionId = table.Column<int>(type: "int", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    TimeTakenSeconds = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackAttemptQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackAttemptQuestions_LearningTrackQuizAttempts_LearningTrackAttemptId",
                        column: x => x.LearningTrackAttemptId,
                        principalTable: "LearningTrackQuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackAttemptQuestions_LearningTrackAttemptId",
                table: "LearningTrackAttemptQuestions",
                column: "LearningTrackAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackQuizAttempts_LearningTrackId",
                table: "LearningTrackQuizAttempts",
                column: "LearningTrackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningTrackAttemptQuestions");

            migrationBuilder.DropTable(
                name: "LearningTrackQuizAttempts");
        }
    }
}
