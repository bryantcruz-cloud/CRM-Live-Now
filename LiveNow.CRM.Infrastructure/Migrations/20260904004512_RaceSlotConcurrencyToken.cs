using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveNow.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RaceSlotConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "RaceSlots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "RaceSlots");
        }
    }
}
