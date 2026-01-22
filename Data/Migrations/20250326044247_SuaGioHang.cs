using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class SuaGioHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Carts_HomestayId",
                table: "Carts",
                column: "HomestayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Homestays_HomestayId",
                table: "Carts",
                column: "HomestayId",
                principalTable: "Homestays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Homestays_HomestayId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_HomestayId",
                table: "Carts");
        }
    }
}
