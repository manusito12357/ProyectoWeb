using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoweb.Migrations
{
    /// <inheritdoc />
    public partial class IndicesUnicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeccionesCompletadas_UserId",
                table: "LeccionesCompletadas");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_UserId",
                table: "Inscripciones");

            migrationBuilder.CreateIndex(
                name: "IX_LeccionesCompletadas_UserId_LeccionId",
                table: "LeccionesCompletadas",
                columns: new[] { "UserId", "LeccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_UserId_CursoId",
                table: "Inscripciones",
                columns: new[] { "UserId", "CursoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeccionesCompletadas_UserId_LeccionId",
                table: "LeccionesCompletadas");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_UserId_CursoId",
                table: "Inscripciones");

            migrationBuilder.CreateIndex(
                name: "IX_LeccionesCompletadas_UserId",
                table: "LeccionesCompletadas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_UserId",
                table: "Inscripciones",
                column: "UserId");
        }
    }
}
