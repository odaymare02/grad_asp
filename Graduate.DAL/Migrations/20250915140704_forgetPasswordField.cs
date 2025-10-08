using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduate.DAL.Migrations
{
    /// <inheritdoc />
    public partial class forgetPasswordField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeResetPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeResetPasswordExpire",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeResetPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CodeResetPasswordExpire",
                table: "Users");
        }
    }
}
