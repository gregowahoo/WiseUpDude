using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WiseUpDude.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Seniors Need To Know");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Fun Facts");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Brain Boosters");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "History Mysteries");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Tech Trends");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Pop Culture Picks");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Travel Treasures");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Health Wellness");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Financial Smarts");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Science Wonders");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Literary Legends");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Everyday Hacks");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Local Lore");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Green Living");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "SeniorsNeedToKnow");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "FunFacts");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "BrainBoosters");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "HistoryMysteries");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "TechTrends");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "PopCulturePicks");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "TravelTreasures");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "HealthWellness");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "FinancialSmarts");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "ScienceWonders");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "LiteraryLegends");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "EverydayHacks");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "LocalLore");

            migrationBuilder.UpdateData(
                table: "AssignmentTypes",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "GreenLiving");
        }
    }
}
