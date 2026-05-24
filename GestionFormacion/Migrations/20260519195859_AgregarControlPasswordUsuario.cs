using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionFormacion.Migrations
{
    /// <inheritdoc />
    public partial class AgregarControlPasswordUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSolicitudReset",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SolicitoResetPassword",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FechaSolicitudReset",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SolicitoResetPassword",
                table: "AspNetUsers");
        }
    }
}
