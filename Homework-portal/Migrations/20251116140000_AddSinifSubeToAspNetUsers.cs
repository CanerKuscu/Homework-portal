using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homework_portal.Migrations
{
    public partial class AddSinifSubeToAspNetUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sinif",
                table: "AspNetUsers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sube",
                table: "AspNetUsers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sinif",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Sube",
                table: "AspNetUsers");
        }
    }
}
