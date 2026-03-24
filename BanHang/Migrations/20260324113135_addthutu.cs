using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class addthutu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KhuVucHienThi",
                table: "SanPhams");

            migrationBuilder.AddColumn<int>(
                name: "KhuVucHienThiId",
                table: "SanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KhuVucHienThis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuVucHienThis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_KhuVucHienThiId",
                table: "SanPhams",
                column: "KhuVucHienThiId");

            migrationBuilder.AddForeignKey(
                name: "FK_SanPhams_KhuVucHienThis_KhuVucHienThiId",
                table: "SanPhams",
                column: "KhuVucHienThiId",
                principalTable: "KhuVucHienThis",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanPhams_KhuVucHienThis_KhuVucHienThiId",
                table: "SanPhams");

            migrationBuilder.DropTable(
                name: "KhuVucHienThis");

            migrationBuilder.DropIndex(
                name: "IX_SanPhams_KhuVucHienThiId",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "KhuVucHienThiId",
                table: "SanPhams");

            migrationBuilder.AddColumn<string>(
                name: "KhuVucHienThi",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
