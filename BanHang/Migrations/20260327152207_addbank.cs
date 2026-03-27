using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class addbank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaThanhToan",
                table: "DonHangs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaChuyenKhoan",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayThanhToan",
                table: "DonHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhuongThucThanhToan",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaThanhToan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "MaChuyenKhoan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "NgayThanhToan",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "PhuongThucThanhToan",
                table: "DonHangs");
        }
    }
}
