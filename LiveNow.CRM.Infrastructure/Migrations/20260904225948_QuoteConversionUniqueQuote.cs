using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveNow.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuoteConversionUniqueQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_QuoteId",
                table: "Sales");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_QuoteId",
                table: "Sales",
                column: "QuoteId",
                unique: true,
                filter: "\"QuoteId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_QuoteId",
                table: "Sales");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_QuoteId",
                table: "Sales",
                column: "QuoteId");
        }
    }
}
