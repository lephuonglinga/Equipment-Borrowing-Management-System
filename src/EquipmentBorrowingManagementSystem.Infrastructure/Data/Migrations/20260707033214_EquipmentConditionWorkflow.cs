using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentConditionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentCondition",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("UPDATE Equipments SET CurrentCondition = 1 WHERE CurrentCondition = 0");

            migrationBuilder.AddColumn<string>(
                name: "HandoverNote",
                table: "BorrowRequestItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnNote",
                table: "BorrowRequestItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCondition",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "HandoverNote",
                table: "BorrowRequestItems");

            migrationBuilder.DropColumn(
                name: "ReturnNote",
                table: "BorrowRequestItems");
        }
    }
}
