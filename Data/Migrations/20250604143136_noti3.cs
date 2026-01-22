using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class noti3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "DeletedHomestayNotifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "DeletedHomestayNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
