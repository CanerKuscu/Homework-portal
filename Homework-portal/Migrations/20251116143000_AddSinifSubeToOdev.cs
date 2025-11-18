using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homework_portal.Migrations
{
    public partial class AddSinifSubeToOdev : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sinif",
                table: "Odevler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sube",
                table: "Odevler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Sinif", table: "Odevler");
            migrationBuilder.DropColumn(name: "Sube", table: "Odevler");
        }
    }
}
