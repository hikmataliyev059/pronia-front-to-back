using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProniaFrontToBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Primary",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Primary",
                table: "ProductImages");
        }
    }
}
