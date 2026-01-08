using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertOrderStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert string statuses to enum integers
            migrationBuilder.Sql("""
                UPDATE Orders SET Status = '0' WHERE Status = 'Created';
                UPDATE Orders SET Status = '1' WHERE Status = 'Paid';
                UPDATE Orders SET Status = '2' WHERE Status = 'Cancelled';
            """);
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("""
                UPDATE Orders SET Status = 'Created' WHERE Status = '0';
                UPDATE Orders SET Status = 'Paid' WHERE Status = '1';
                UPDATE Orders SET Status = 'Cancelled' WHERE Status = '2';
            """);
        }
    }
}
