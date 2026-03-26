using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecenPorMi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePeticionesYRezos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Peticiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContadorRezos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peticiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rezos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeticionId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezos_Peticiones_PeticionId",
                        column: x => x.PeticionId,
                        principalTable: "Peticiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Peticiones_FechaPublicacion",
                table: "Peticiones",
                column: "FechaPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Rezos_PeticionId_IpHash_Fecha",
                table: "Rezos",
                columns: new[] { "PeticionId", "IpHash", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rezos");

            migrationBuilder.DropTable(
                name: "Peticiones");
        }
    }
}
