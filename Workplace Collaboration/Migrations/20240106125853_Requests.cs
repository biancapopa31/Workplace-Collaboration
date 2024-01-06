using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workplace_Collaboration.Migrations
{
    public partial class Requests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRequestedToJoinChannel",
                columns: table => new
                {
                    RequestedChannelId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestedToJoinChannel", x => new { x.RequestedChannelId, x.RequesterId });
                    table.ForeignKey(
                        name: "FK_UserRequestedToJoinChannel_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRequestedToJoinChannel_Channels_RequestedChannelId",
                        column: x => x.RequestedChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestedToJoinChannel_RequesterId",
                table: "UserRequestedToJoinChannel",
                column: "RequesterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRequestedToJoinChannel");
        }
    }
}
