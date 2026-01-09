using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add indexes on frequently queried columns for better performance
            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs",
                column: "CreatedAt",
                descending: new[] { true });

            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_ResponseStatusCode",
                table: "RequestResponseLogs",
                column: "ResponseStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_Method",
                table: "RequestResponseLogs",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_CreatedAt_ResponseStatusCode",
                table: "RequestResponseLogs",
                columns: new[] { "CreatedAt", "ResponseStatusCode" },
                descending: new[] { true, false });

            // Add index on Path for searching
            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_Path",
                table: "RequestResponseLogs",
                column: "Path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_ResponseStatusCode",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_Method",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_CreatedAt_ResponseStatusCode",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_Path",
                table: "RequestResponseLogs");
        }
    }
}
