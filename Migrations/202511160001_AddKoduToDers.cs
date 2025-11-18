using System.ComponentModel.DataAnnotations.Schema;

public class Ders
{
    // ...
    [Column("Kod")] // map to existing column name
    public string? Kodu { get; set; }
}using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homework_portal.Migrations
{
    public partial class AddKoduToDers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adjust the table name if yours is not "Ders"
            migrationBuilder.AddColumn<string>(
                name: "Kodu",
                table: "Ders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kodu",
                table: "Ders");
        }
    }
}