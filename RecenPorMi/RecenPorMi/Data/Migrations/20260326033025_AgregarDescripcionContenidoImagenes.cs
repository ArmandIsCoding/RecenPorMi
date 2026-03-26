using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecenPorMi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDescripcionContenidoImagenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Paso 1: Truncar valores de Contenido que excedan 200 caracteres
            migrationBuilder.Sql(@"
                UPDATE Peticiones 
                SET Contenido = LEFT(Contenido, 200) 
                WHERE LEN(Contenido) > 200
            ");

            // Paso 2: Renombrar columna Contenido a DescripcionBreve
            migrationBuilder.RenameColumn(
                name: "Contenido",
                table: "Peticiones",
                newName: "DescripcionBreve");

            // Paso 3: Modificar la columna para establecer MaxLength 200
            migrationBuilder.AlterColumn<string>(
                name: "DescripcionBreve",
                table: "Peticiones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            // Paso 4: Agregar nueva columna ContenidoCompleto (opcional)
            migrationBuilder.AddColumn<string>(
                name: "ContenidoCompleto",
                table: "Peticiones",
                type: "nvarchar(max)",
                nullable: true);

            // Paso 5: Crear tabla PeticionImagenes
            migrationBuilder.CreateTable(
                name: "PeticionImagenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeticionId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RutaImagen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeticionImagenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeticionImagenes_Peticiones_PeticionId",
                        column: x => x.PeticionId,
                        principalTable: "Peticiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeticionImagenes_PeticionId",
                table: "PeticionImagenes",
                column: "PeticionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeticionImagenes");

            migrationBuilder.DropColumn(
                name: "ContenidoCompleto",
                table: "Peticiones");

            // Renombrar DescripcionBreve de vuelta a Contenido
            migrationBuilder.RenameColumn(
                name: "DescripcionBreve",
                table: "Peticiones",
                newName: "Contenido");

            // Restaurar el tipo original
            migrationBuilder.AlterColumn<string>(
                name: "Contenido",
                table: "Peticiones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
