using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningTrackEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTracks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrackCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LearningTrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackCategories_LearningTracks_LearningTrackId",
                        column: x => x.LearningTrackId,
                        principalTable: "LearningTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrackSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LearningTrackCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackSources_LearningTrackCategories_LearningTrackCategoryId",
                        column: x => x.LearningTrackCategoryId,
                        principalTable: "LearningTrackCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrackQuizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LearningTrackSourceId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackQuizzes_LearningTrackSources_LearningTrackSourceId",
                        column: x => x.LearningTrackSourceId,
                        principalTable: "LearningTrackSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningTrackQuizQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearningTrackQuizId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OptionsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTrackQuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTrackQuizQuestions_LearningTrackQuizzes_LearningTrackQuizId",
                        column: x => x.LearningTrackQuizId,
                        principalTable: "LearningTrackQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackCategories_LearningTrackId",
                table: "LearningTrackCategories",
                column: "LearningTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackQuizQuestions_LearningTrackQuizId",
                table: "LearningTrackQuizQuestions",
                column: "LearningTrackQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackQuizzes_LearningTrackSourceId",
                table: "LearningTrackQuizzes",
                column: "LearningTrackSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTracks_UserId",
                table: "LearningTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTrackSources_LearningTrackCategoryId",
                table: "LearningTrackSources",
                column: "LearningTrackCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningTrackQuizQuestions");

            migrationBuilder.DropTable(
                name: "LearningTrackQuizzes");

            migrationBuilder.DropTable(
                name: "LearningTrackSources");

            migrationBuilder.DropTable(
                name: "LearningTrackCategories");

            migrationBuilder.DropTable(
                name: "LearningTracks");
        }
    }
}
