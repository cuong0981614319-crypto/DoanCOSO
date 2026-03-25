using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class addHinhAnh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_MaDanhMuc",
                table: "SanPhams");

            migrationBuilder.AddColumn<int>(
                name: "DanhMucMaDanhMuc",
                table: "SanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HinhAnhSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSanPham = table.Column<int>(type: "int", nullable: false),
                    DuongDanAnh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPhams_SanPhams_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPhams",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_DanhMucMaDanhMuc",
                table: "SanPhams",
                column: "DanhMucMaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPhams_MaSanPham",
                table: "HinhAnhSanPhams",
                column: "MaSanPham");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_DanhMucs_DanhMucMaDanhMuc",
                table: "SanPhams",
                column: "DanhMucMaDanhMuc",
                principalTable: "DanhMucs",
                principalColumn: "MaDanhMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_DanhMucs_DanhMucMaDanhMuc",
                table: "SanPhams");

            migrationBuilder.DropTable(
                name: "HinhAnhSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_DanhMucMaDanhMuc",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "DanhMucMaDanhMuc",
                table: "SanPhams");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_MaDanhMuc",
                table: "SanPhams",
                column: "MaDanhMuc");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_DanhMucs_MaDanhMuc",
                table: "SanPhams",
                column: "MaDanhMuc",
                principalTable: "DanhMucs",
                principalColumn: "MaDanhMuc",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
