using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class staffffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Homestays",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Homestays_UserId",
                table: "Homestays",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Homestays_AspNetUsers_UserId",
                table: "Homestays",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Homestays_AspNetUsers_UserId",
                table: "Homestays");

            migrationBuilder.DropIndex(
                name: "IX_Homestays_UserId",
                table: "Homestays");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Homestays");
        }
    }
}
