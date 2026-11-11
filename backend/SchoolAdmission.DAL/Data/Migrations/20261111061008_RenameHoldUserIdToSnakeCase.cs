using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolAdmission.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameHoldUserIdToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_holds_AspNetUsers_UserId",
                table: "holds");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "holds",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_holds_UserId",
                table: "holds",
                newName: "IX_holds_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_holds_AspNetUsers_user_id",
                table: "holds",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_holds_AspNetUsers_user_id",
                table: "holds");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "holds",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_holds_user_id",
                table: "holds",
                newName: "IX_holds_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_holds_AspNetUsers_UserId",
                table: "holds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
