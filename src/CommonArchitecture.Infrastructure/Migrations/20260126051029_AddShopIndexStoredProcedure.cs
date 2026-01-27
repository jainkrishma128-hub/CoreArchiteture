using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopIndexStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_GetShopIndexData
AS
BEGIN
    SET NOCOUNT ON;
    WITH ActiveCategories AS (
        SELECT Id, Name FROM Categories WHERE IsActive = 1
    ),
    RankedProducts AS (
        SELECT 
            p.Id, p.Name, p.Description, p.Price, p.CategoryId,
            cat.Name as CategoryName,
            ISNULL((SELECT SUM(Quantity) FROM InventoryTransactions WHERE ProductId = p.Id), 0) as Stock,
            ROW_NUMBER() OVER (PARTITION BY p.CategoryId ORDER BY p.Id DESC) as Rank
        FROM Products p
        JOIN ActiveCategories cat ON p.CategoryId = cat.Id
    )
    SELECT Id, Name, Description, Price, CategoryId, CategoryName, Stock 
    FROM RankedProducts 
    WHERE Rank <= 4;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetShopIndexData");
        }
    }
}
