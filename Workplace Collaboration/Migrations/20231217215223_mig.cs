using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplace_Collaboration.Migrations
{
    public partial class mig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryID_ChannelId_CategoryId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChannelHasCategoryID",
                table: "Messages",
                newName: "ChannelHasCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChannelHasCategoryID_ChannelId_CategoryId",
                table: "Messages",
                newName: "IX_Messages_ChannelHasCategoryId_ChannelId_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryId", "ChannelId", "CategoryId" },
                principalTable: "ChannelHasCategories",
                principalColumns: new[] { "Id", "CategoryId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChannelHasCategoryId",
                table: "Messages",
                newName: "ChannelHasCategoryID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages",
                newName: "IX_Messages_ChannelHasCategoryID_ChannelId_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryID_ChannelId_CategoryId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryID", "ChannelId", "CategoryId" },
                principalTable: "ChannelHasCategories",
                principalColumns: new[] { "Id", "CategoryId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
