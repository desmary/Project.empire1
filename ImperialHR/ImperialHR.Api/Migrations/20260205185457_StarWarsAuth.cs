using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImperialHR.Api.Migrations
{
    /// <inheritdoc />
    public partial class StarWarsAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Employees_ApproverId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Employees_EmployeeId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ApproverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ApproverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DecidedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DecisionComment",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Requests",
                newName: "FinalTo");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Requests",
                newName: "FinalFrom");

            migrationBuilder.AddColumn<DateTime>(
                name: "From",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "To",
                table: "Requests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Employees_EmployeeId",
                table: "Requests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Employees_EmployeeId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "From",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "To",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "FinalTo",
                table: "Requests",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "FinalFrom",
                table: "Requests",
                newName: "EndDate");

            migrationBuilder.AddColumn<int>(
                name: "ApproverId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Requests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAt",
                table: "Requests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionComment",
                table: "Requests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ApproverId",
                table: "Requests",
                column: "ApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Employees_ApproverId",
                table: "Requests",
                column: "ApproverId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Employees_EmployeeId",
                table: "Requests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
