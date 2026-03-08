using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HmDianPing.Migrations
{
    /// <inheritdoc />
    public partial class InitialShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "tb_shop",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "tb_shop",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RecommendedDishes",
                table: "tb_shop",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReviewSummary",
                table: "tb_shop",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "tb_shop");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "tb_shop");

            migrationBuilder.DropColumn(
                name: "RecommendedDishes",
                table: "tb_shop");

            migrationBuilder.DropColumn(
                name: "ReviewSummary",
                table: "tb_shop");
        }
    }
}
