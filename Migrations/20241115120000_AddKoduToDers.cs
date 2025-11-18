using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
public partial class AddKoduToDers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Kodu",
            table: "Dersler",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Kodu",
            table: "Dersler");
    }
}   using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
public partial class AddKoduToDers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Kodu",
            table: "Dersler",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Kodu",
            table: "Dersler");
    }
}

[Column("DersKodu")]
[StringLength(30)]
public string? Kodu { get; set; }