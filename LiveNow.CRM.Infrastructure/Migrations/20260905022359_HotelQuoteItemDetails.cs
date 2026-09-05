using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveNow.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HotelQuoteItemDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoardBasis",
                table: "SaleItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "SaleItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOut",
                table: "SaleItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HotelId",
                table: "SaleItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Nights",
                table: "SaleItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "SaleItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Occupancy",
                table: "SaleItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationPolicy",
                table: "SaleItems",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomType",
                table: "SaleItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "SaleItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoardBasis",
                table: "QuoteItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "QuoteItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOut",
                table: "QuoteItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HotelId",
                table: "QuoteItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Nights",
                table: "QuoteItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRooms",
                table: "QuoteItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Occupancy",
                table: "QuoteItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationPolicy",
                table: "QuoteItems",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomType",
                table: "QuoteItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "QuoteItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardBasis",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CheckOut",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Nights",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "Occupancy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "ReservationPolicy",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "BoardBasis",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "CheckOut",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "Nights",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "NumberOfRooms",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "Occupancy",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "ReservationPolicy",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "QuoteItems");
        }
    }
}
