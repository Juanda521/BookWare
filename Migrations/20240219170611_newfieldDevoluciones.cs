using Microsoft.EntityFrameworkCore.Migrations;


namespace tallerbiblioteca.Migrations
{
    // / <inheritdoc />
    public partial class newfieldDevoluciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Devoluciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Devoluciones");
        }
    }
}
