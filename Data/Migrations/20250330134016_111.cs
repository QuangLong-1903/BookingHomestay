using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class _111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userActivityLogs_AspNetUsers_UserId",
                table: "userActivityLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userActivityLogs",
                table: "userActivityLogs");

            migrationBuilder.RenameTable(
                name: "userActivityLogs",
                newName: "UserActivityLogs");

            migrationBuilder.RenameIndex(
                name: "IX_userActivityLogs_UserId",
                table: "UserActivityLogs",
                newName: "IX_UserActivityLogs_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivityLogs",
                table: "UserActivityLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_AspNetUsers_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_AspNetUsers_UserId",
                table: "UserActivityLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivityLogs",
                table: "UserActivityLogs");

            migrationBuilder.RenameTable(
                name: "UserActivityLogs",
                newName: "userActivityLogs");

            migrationBuilder.RenameIndex(
                name: "IX_UserActivityLogs_UserId",
                table: "userActivityLogs",
                newName: "IX_userActivityLogs_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userActivityLogs",
                table: "userActivityLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userActivityLogs_AspNetUsers_UserId",
                table: "userActivityLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
