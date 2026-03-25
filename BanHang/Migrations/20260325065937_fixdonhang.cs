using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class fixdonhang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChiGiaoHang",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DonHangs");

            migrationBuilder.RenameColumn(
                name: "PhuongThucThanhToan",
                table: "DonHangs",
                newName: "HoTen");

            migrationBuilder.RenameColumn(
                name: "HoTenNguoiNhan",
                table: "DonHangs",
                newName: "DiaChi");

            migrationBuilder.RenameColumn(
                name: "MaChiTietDonHang",
                table: "ChiTietDonHangs",
                newName: "MaChiTiet");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HoTen",
                table: "DonHangs",
                newName: "PhuongThucThanhToan");

            migrationBuilder.RenameColumn(
                name: "DiaChi",
                table: "DonHangs",
                newName: "HoTenNguoiNhan");

            migrationBuilder.RenameColumn(
                name: "MaChiTiet",
                table: "ChiTietDonHangs",
                newName: "MaChiTietDonHang");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiGiaoHang",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
