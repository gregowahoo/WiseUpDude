using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreationDateToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "UserQuizzes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "UserQuizQuestions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Topics",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Quizzes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "QuizQuestions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "UserQuizzes");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "UserQuizQuestions");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "QuizQuestions");
        }
    }
}
