using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventAccessControl.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInLogs_Tickets_TicketId",
                table: "CheckInLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "CheckInLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInLogs_Tickets_TicketId",
                table: "CheckInLogs",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInLogs_Tickets_TicketId",
                table: "CheckInLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "TicketId",
                table: "CheckInLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInLogs_Tickets_TicketId",
                table: "CheckInLogs",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
