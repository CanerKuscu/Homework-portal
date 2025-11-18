using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homework_portal.Migrations
{
    /// <inheritdoc />
    public partial class ModelGuncellendiKoduKaldirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DersKayitlari_AspNetUsers_OgrenciId",
                table: "DersKayitlari");

            migrationBuilder.AddColumn<string>(
                name: "Kod",
                table: "Dersler",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DersKayitlari_AspNetUsers_OgrenciId",
                table: "DersKayitlari",
                column: "OgrenciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DersKayitlari_AspNetUsers_OgrenciId",
                table: "DersKayitlari");

            migrationBuilder.DropColumn(
                name: "Kod",
                table: "Dersler");

            migrationBuilder.AddForeignKey(
                name: "FK_DersKayitlari_AspNetUsers_OgrenciId",
                table: "DersKayitlari",
                column: "OgrenciId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
