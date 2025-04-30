using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndCategoryDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Topics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryDescription",
                table: "Topics",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "CategoryDescription",
                table: "Topics");
        }
    }
}
