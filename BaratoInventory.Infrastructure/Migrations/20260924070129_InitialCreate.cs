using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BaratoInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAtUtc", "IsDeleted", "Name", "Price", "Quantity", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "Beverages", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Coke 1.5L", 65.00m, 100, null },
                    { 2, "Bakery", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Gardenia Classic White Bread", 75.00m, 50, null },
                    { 3, "Dairy", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Alaska Fresh Milk 1L", 95.00m, 30, null },
                    { 4, "Canned Goods", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Century Tuna Flakes in Oil", 35.00m, 200, null },
                    { 5, "Snacks", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Piattos Cheese 85g", 38.00m, 150, null },
                    { 6, "Household", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Ariel Powder 1kg", 250.00m, 45, null },
                    { 7, "Household", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Joy Dishwashing Liquid 500ml", 110.00m, 80, null },
                    { 8, "Beverages", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Nescafé Classic 200g", 155.00m, 120, null },
                    { 9, "Frozen", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Purefoods Tender Juicy Hotdog 1kg", 320.00m, 60, null },
                    { 10, "Condiments", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Datu Puti Soy Sauce 1L", 45.00m, 300, null },
                    { 11, "Condiments", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Mang Tomas Sarsa 330g", 55.00m, 140, null },
                    { 12, "Canned Goods", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Ligo Sardines in Tomato Sauce 155g", 22.00m, 500, null },
                    { 13, "Beverages", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Kopiko Blanca Twin Pack", 15.00m, 400, null },
                    { 14, "Noodles", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lucky Me! Pancit Canton Kalamansi", 18.00m, 600, null },
                    { 15, "Meat", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Bounty Fresh Whole Chicken", 180.00m, 25, null },
                    { 16, "Dairy", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Magnolia Gold Butter 200g", 125.00m, 40, null },
                    { 17, "Snacks", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Pringles Original 149g", 105.00m, 85, null },
                    { 18, "Personal Care", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Colgate Double Action Toothbrush", 65.00m, 110, null },
                    { 19, "Personal Care", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Safeguard Pure White Soap 130g", 45.00m, 250, null },
                    { 20, "Snacks", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), false, "Fita Crackers 250g", 55.00m, 90, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
