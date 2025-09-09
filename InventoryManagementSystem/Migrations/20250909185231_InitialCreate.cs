using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatewiseProductions",
                columns: table => new
                {
                    ProdID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Production = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Sell = table.Column<int>(type: "int", nullable: false),
                    Waste = table.Column<int>(type: "int", nullable: false),
                    LeftOver = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatewiseProductions", x => x.ProdID);
                });

            migrationBuilder.CreateTable(
                name: "ItemRecipes",
                columns: table => new
                {
                    ItemRecipeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IngredientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityGmPerPc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityPcPerPc = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemRecipes", x => x.ItemRecipeID);
                });

            migrationBuilder.CreateTable(
                name: "MTDProductions",
                columns: table => new
                {
                    MTDProductionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalProduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSell = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWaste = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLeftOver = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTDProductions", x => x.MTDProductionID);
                });

            migrationBuilder.CreateTable(
                name: "RemainingStocks",
                columns: table => new
                {
                    RemainingStockID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IngredientLeftOver = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemainingStocks", x => x.RemainingStockID);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockUnit = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatewiseProductions");

            migrationBuilder.DropTable(
                name: "ItemRecipes");

            migrationBuilder.DropTable(
                name: "MTDProductions");

            migrationBuilder.DropTable(
                name: "RemainingStocks");

            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
