using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialQuizAssignmentAndAssignmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialQuizAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserQuizId = table.Column<int>(type: "int", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignmentTypeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialQuizAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialQuizAssignments_AssignmentTypes_AssignmentTypeId",
                        column: x => x.AssignmentTypeId,
                        principalTable: "AssignmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecialQuizAssignments_UserQuizzes_UserQuizId",
                        column: x => x.UserQuizId,
                        principalTable: "UserQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AssignmentTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Quizzes selected for special prominence.", "Featured" },
                    { 2, "Practical, health, safety, and life skills quizzes for older adults.", "SeniorsNeedToKnow" },
                    { 3, "Quizzes packed with surprising, quirky, or little-known facts.", "FunFacts" },
                    { 4, "Quizzes with mind-blowing, record-breaking, or 'did you know?' content.", "Wow" },
                    { 5, "Puzzles, logic, and memory challenges to keep minds sharp.", "BrainBoosters" },
                    { 6, "Quizzes about unsolved historical events or famous mysteries.", "HistoryMysteries" },
                    { 7, "Quizzes about the latest in technology, gadgets, and digital life.", "TechTrends" },
                    { 8, "Quizzes on movies, music, celebrities, and viral trends.", "PopCulturePicks" },
                    { 9, "Quizzes about world geography, cultures, and travel tips.", "TravelTreasures" },
                    { 10, "Nutrition, exercise, and mental health quizzes.", "HealthWellness" },
                    { 11, "Quizzes on saving, investing, and money management.", "FinancialSmarts" },
                    { 12, "Quizzes about amazing discoveries and scientific phenomena.", "ScienceWonders" },
                    { 13, "Quizzes on classic books, authors, and literary trivia.", "LiteraryLegends" },
                    { 14, "Quizzes with tips and tricks for daily life.", "EverydayHacks" },
                    { 15, "Quizzes about local history, landmarks, and traditions.", "LocalLore" },
                    { 16, "Quizzes on sustainability, nature, and eco-friendly habits.", "GreenLiving" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialQuizAssignments_AssignmentTypeId",
                table: "SpecialQuizAssignments",
                column: "AssignmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialQuizAssignments_UserQuizId",
                table: "SpecialQuizAssignments",
                column: "UserQuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialQuizAssignments");

            migrationBuilder.DropTable(
                name: "AssignmentTypes");

        }
    }
}
