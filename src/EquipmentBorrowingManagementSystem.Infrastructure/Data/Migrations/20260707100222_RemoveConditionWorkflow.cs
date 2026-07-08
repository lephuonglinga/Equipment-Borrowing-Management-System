using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConditionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallCondition",
                table: "ReturnRecords");

            migrationBuilder.DropColumn(
                name: "CurrentCondition",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "ConditionAtBorrow",
                table: "BorrowRequestItems");

            migrationBuilder.DropColumn(
                name: "ConditionAtReturn",
                table: "BorrowRequestItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OverallCondition",
                table: "ReturnRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentCondition",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConditionAtBorrow",
                table: "BorrowRequestItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConditionAtReturn",
                table: "BorrowRequestItems",
                type: "int",
                nullable: true);
        }
    }
}
