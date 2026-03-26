using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecenPorMi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAutenticacionAPeticiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ DESARROLLO: Limpiar datos existentes antes de modificar estructura
            migrationBuilder.Sql("DELETE FROM [Rezos];");
            migrationBuilder.Sql("DELETE FROM [Peticiones];");

            migrationBuilder.DropColumn(
                name: "Alias",
                table: "Peticiones");

            migrationBuilder.AddColumn<bool>(
                name: "PublicarAnonimamente",
                table: "Peticiones",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Peticiones",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Peticiones_UserId",
                table: "Peticiones",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Peticiones_AspNetUsers_UserId",
                table: "Peticiones",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peticiones_AspNetUsers_UserId",
                table: "Peticiones");

            migrationBuilder.DropIndex(
                name: "IX_Peticiones_UserId",
                table: "Peticiones");

            migrationBuilder.DropColumn(
                name: "PublicarAnonimamente",
                table: "Peticiones");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Peticiones");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "Peticiones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
