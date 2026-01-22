using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class Report1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Homestays_AspNetUsers_StaffId",
                table: "Homestays");

            migrationBuilder.DropIndex(
                name: "IX_Homestays_StaffId",
                table: "Homestays");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "Homestays",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "Homestays",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Homestays_StaffId",
                table: "Homestays",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Homestays_AspNetUsers_StaffId",
                table: "Homestays",
                column: "StaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
