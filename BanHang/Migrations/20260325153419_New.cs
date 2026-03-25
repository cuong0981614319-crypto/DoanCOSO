using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoLuong",
                table: "SanPhams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoLuong",
                table: "SanPhams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
