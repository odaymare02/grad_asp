using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduate.DAL.Migrations
{
    /// <inheritdoc />
    public partial class majortocahtrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Major",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "unknown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Major",
                table: "ChatMessages");
        }
    }
}
