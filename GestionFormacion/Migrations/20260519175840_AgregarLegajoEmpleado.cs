using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionFormacion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLegajoEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Legajo",
                table: "Empleados",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Legajo",
                table: "Empleados");
        }
    }
}
