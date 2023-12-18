using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplace_Collaboration.Migrations
{
    public partial class modMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryID_ChannelID_CategoryID",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChannelID",
                table: "Messages",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "Messages",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChannelHasCategoryID_ChannelID_CategoryID",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryID_ChannelId_CategoryId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Messages",
                newName: "ChannelID");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Messages",
                newName: "CategoryID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChannelHasCategoryID_ChannelId_CategoryId",
                table: "Messages",
                newName: "IX_Messages_ChannelHasCategoryID_ChannelID_CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryID_ChannelID_CategoryID",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryID", "ChannelID", "CategoryID" },
                principalTable: "ChannelHasCategories",
                principalColumns: new[] { "Id", "CategoryId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
