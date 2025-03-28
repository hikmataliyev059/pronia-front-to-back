using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProniaFrontToBack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableeColumnnn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpireTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCodeExpireTime",
                table: "AspNetUsers");
        }
    }
}
