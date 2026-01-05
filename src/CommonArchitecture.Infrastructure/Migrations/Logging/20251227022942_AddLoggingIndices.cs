using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations.Logging
{
    /// <inheritdoc />
    public partial class AddLoggingIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_Method",
                table: "RequestResponseLogs",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_ResponseStatusCode",
                table: "RequestResponseLogs",
                column: "ResponseStatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_Method",
                table: "RequestResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_ResponseStatusCode",
                table: "RequestResponseLogs");
        }
    }
}
