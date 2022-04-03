using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccess.Migrations
{
    public partial class TypoCorrection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoItems_AspNetUsers_AsigneeId",
                table: "ToDoItems");

            migrationBuilder.RenameColumn(
                name: "AsigneeId",
                table: "ToDoItems",
                newName: "AssigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoItems_AsigneeId",
                table: "ToDoItems",
                newName: "IX_ToDoItems_AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoItems_AspNetUsers_AssigneeId",
                table: "ToDoItems",
                column: "AssigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoItems_AspNetUsers_AssigneeId",
                table: "ToDoItems");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "ToDoItems",
                newName: "AsigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoItems_AssigneeId",
                table: "ToDoItems",
                newName: "IX_ToDoItems_AsigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoItems_AspNetUsers_AsigneeId",
                table: "ToDoItems",
                column: "AsigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
