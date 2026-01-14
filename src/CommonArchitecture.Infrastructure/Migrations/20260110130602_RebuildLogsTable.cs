using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebuildLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             // 1. Create Table (if not exists - though migration logic implies it's new to EF, we make sure)
            migrationBuilder.CreateTable(
                name: "RequestResponseLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Method = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: false),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestResponseLogs", x => x.Id);
                });

             // 2. Add Indexes
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
                
           // 3. Create Stored Procedure
           migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_GetDashboardStats
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Daily Stats
    SELECT 
        CAST(CreatedAt AS DATE) as [Date], 
        COUNT(1) as TotalRequests, 
        AVG(CAST(DurationMs AS FLOAT)) as AverageDuration
    FROM RequestResponseLogs
    WHERE CreatedAt >= @FromDate AND CreatedAt <= @ToDate
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY [Date];

    -- Result Set 2: Status Distribution
    SELECT 
        SUM(CASE WHEN ResponseStatusCode >= 200 AND ResponseStatusCode < 300 THEN 1 ELSE 0 END) as Success,
        SUM(CASE WHEN ResponseStatusCode >= 400 AND ResponseStatusCode < 500 THEN 1 ELSE 0 END) as ClientError,
        SUM(CASE WHEN ResponseStatusCode >= 500 THEN 1 ELSE 0 END) as ServerError
    FROM RequestResponseLogs
    WHERE CreatedAt >= @FromDate AND CreatedAt <= @ToDate;

    -- Result Set 3: Overall Average Duration
    SELECT AVG(CAST(DurationMs AS FLOAT)) as OverallAverage
    FROM RequestResponseLogs
    WHERE CreatedAt >= @FromDate AND CreatedAt <= @ToDate;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetDashboardStats");
             migrationBuilder.DropTable(name: "RequestResponseLogs");
        }
    }
}
