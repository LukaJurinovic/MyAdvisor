using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodToRecurringTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "RecurringTransactions",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "RecurringTransactions");
        }
    }
}
