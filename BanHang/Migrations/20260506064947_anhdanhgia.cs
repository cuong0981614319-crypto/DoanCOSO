using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHang.Migrations
{
    /// <inheritdoc />
    public partial class anhdanhgia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhGiaImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanhGiaId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaImages_DanhGias_DanhGiaId",
                        column: x => x.DanhGiaId,
                        principalTable: "DanhGias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaImages_DanhGiaId",
                table: "DanhGiaImages",
                column: "DanhGiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGiaImages");
        }
    }
}
