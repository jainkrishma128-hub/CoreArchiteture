using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the simple index if it exists
            migrationBuilder.DropIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs");
                
            // Create covering index: Indexed by CreatedAt, Includes DurationMs
            // This allows the query to get date AND duration without visiting the main table (Key Lookup)
            migrationBuilder.Sql("CREATE INDEX [IX_RequestResponseLogs_CreatedAt_Include_Duration] ON [RequestResponseLogs] ([CreatedAt] DESC) INCLUDE ([DurationMs])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_RequestResponseLogs_CreatedAt_Include_Duration] ON [RequestResponseLogs]");
             
             migrationBuilder.CreateIndex(
                name: "IX_RequestResponseLogs_CreatedAt",
                table: "RequestResponseLogs",
                column: "CreatedAt",
                descending: new[] { true });
        }
    }
}
