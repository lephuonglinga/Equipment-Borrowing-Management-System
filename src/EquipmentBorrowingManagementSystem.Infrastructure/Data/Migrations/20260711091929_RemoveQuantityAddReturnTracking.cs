using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuantityAddReturnTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BorrowRequestItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualReturnDate",
                table: "BorrowRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnStatus",
                table: "BorrowRequestItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualReturnDate",
                table: "BorrowRequests");

            migrationBuilder.DropColumn(
                name: "ReturnStatus",
                table: "BorrowRequestItems");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BorrowRequestItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
