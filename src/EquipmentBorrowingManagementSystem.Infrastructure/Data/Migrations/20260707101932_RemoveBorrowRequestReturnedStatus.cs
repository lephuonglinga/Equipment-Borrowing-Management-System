using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBorrowRequestReturnedStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy: Returned=6, Completed=7, Overdue=8 -> Completed=6, Overdue=7
            migrationBuilder.Sql("""
                UPDATE BorrowRequests SET Status = Status + 100 WHERE Status IN (6, 7, 8);
                UPDATE BorrowRequests SET Status = 6 WHERE Status IN (106, 107);
                UPDATE BorrowRequests SET Status = 7 WHERE Status = 108;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE BorrowRequests SET Status = Status + 100 WHERE Status IN (6, 7);
                UPDATE BorrowRequests SET Status = 7 WHERE Status = 106;
                UPDATE BorrowRequests SET Status = 8 WHERE Status = 107;
                """);
        }
    }
}
