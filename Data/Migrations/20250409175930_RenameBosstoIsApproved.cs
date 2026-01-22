using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCoSo_Nhom2.Migrations
{
    /// <inheritdoc />
    public partial class RenameBosstoIsApproved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Boss",
                table: "Homestays",
                newName: "IsApproved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsApproved",
                table: "Homestays",
                newName: "Boss");
        }
    }
}
