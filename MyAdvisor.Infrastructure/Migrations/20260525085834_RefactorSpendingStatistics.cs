using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAdvisor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSpendingStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryStatistics_Categories_CategoryId",
                table: "CategoryStatistics");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalIncome",
                table: "SpendingStatistics",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CategoryStatistics",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "CategoryStatistics",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsIncome",
                table: "CategoryStatistics",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "CategoryStatistics",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryStatistics_Categories_CategoryId",
                table: "CategoryStatistics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryStatistics_Categories_CategoryId",
                table: "CategoryStatistics");

            migrationBuilder.DropColumn(
                name: "TotalIncome",
                table: "SpendingStatistics");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "CategoryStatistics");

            migrationBuilder.DropColumn(
                name: "IsIncome",
                table: "CategoryStatistics");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "CategoryStatistics");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "CategoryStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryStatistics_Categories_CategoryId",
                table: "CategoryStatistics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
