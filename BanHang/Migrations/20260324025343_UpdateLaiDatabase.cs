using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLaiDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_NguoiDungs_MaNguoiDung",
                table: "DonHangs");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropIndex(
                name: "IX_DonHangs_MaNguoiDung",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "MaNguoiDung",
                table: "DonHangs");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "DonHangs",
                newName: "SoDienThoai");

            migrationBuilder.RenameColumn(
                name: "Gia",
                table: "ChiTietDonHangs",
                newName: "DonGia");

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
                name: "GhiChu",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HoTenNguoiNhan",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChiGiaoHang",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "HoTenNguoiNhan",
                table: "DonHangs");

            migrationBuilder.RenameColumn(
                name: "SoDienThoai",
                table: "DonHangs",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "DonGia",
                table: "ChiTietDonHangs",
                newName: "Gia");

            migrationBuilder.RenameColumn(
                name: "MaChiTietDonHang",
                table: "ChiTietDonHangs",
                newName: "MaChiTiet");

            migrationBuilder.AddColumn<int>(
                name: "MaNguoiDung",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToans_DonHangs_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHangs",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaNguoiDung",
                table: "DonHangs",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_MaDonHang",
                table: "ThanhToans",
                column: "MaDonHang");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_NguoiDungs_MaNguoiDung",
                table: "DonHangs",
                column: "MaNguoiDung",
                principalTable: "NguoiDungs",
                principalColumn: "MaNguoiDung",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
