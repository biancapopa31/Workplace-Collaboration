using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplace_Collaboration.Migrations
{
    public partial class channelHasCatFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "ChannelId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChannelHasCategoryId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelHasCategoryId_CategoryId_ChannelId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryId", "CategoryId", "ChannelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_CategoryId_ChannelId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryId", "CategoryId", "ChannelId" },
                principalTable: "ChannelHasCategories",
                principalColumns: new[] { "Id", "CategoryId", "ChannelId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_CategoryId_ChannelId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelHasCategoryId_CategoryId_ChannelId",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "ChannelId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ChannelHasCategoryId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryId", "ChannelId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChannelHasCategories_ChannelHasCategoryId_ChannelId_CategoryId",
                table: "Messages",
                columns: new[] { "ChannelHasCategoryId", "ChannelId", "CategoryId" },
                principalTable: "ChannelHasCategories",
                principalColumns: new[] { "Id", "CategoryId", "ChannelId" });
        }
    }
}
