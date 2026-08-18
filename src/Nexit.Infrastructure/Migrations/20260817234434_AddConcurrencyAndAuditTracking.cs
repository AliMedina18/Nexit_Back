using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyAndAuditTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nota: "xmin" NO se agrega aquí a propósito. Es una columna de sistema que Postgres
            // ya trae en todas las tablas (se usa para control de concurrencia MVCC internamente);
            // EF Core la mapea como shadow property vía HasColumnName("xmin") + IsRowVersion(),
            // pero un "ADD COLUMN xmin" fallaría porque el nombre ya existe como columna reservada.
            // Solo agregamos "updated_by", que sí es una columna nueva de nuestro modelo.
            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "proyectos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "proveedores",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "clientes",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "proyectos");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "proveedores");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "clientes");
        }
    }
}
