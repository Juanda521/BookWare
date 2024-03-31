using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tallerbiblioteca.Migrations
{
    /// <inheritdoc />
    public partial class Reservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.CreateTable(
              name: "Reserva",
              columns: table => new
              {
                  Id = table.Column<int>(type: "int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  IdEjemplar = table.Column<int>(type: "int", nullable: false),
                  IdUsuario = table.Column<int>(type: "int", nullable: false),
                  fechaReserva = table.Column<DateTime>(type: "dateTime", nullable: false),
                  Estado= table.Column<string>(type: "nvarchar(max)", nullable: false)
              }, constraints: table =>
              {
                  table.PrimaryKey("PK_Reservas", x => x.Id);
                  table.ForeignKey(
                       name: "FK_Reservas_Usuario",
                       column: x => x.IdUsuario,
                       principalTable: "Usuarios",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
                  table.ForeignKey(
                       name: "FK_Reservas_ejemplar",
                       column: x => x.IdEjemplar,
                       principalTable: "Ejemplares",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
              }


            );

             

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
