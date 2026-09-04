using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteUbicacionCatalogoYEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ciudad_id",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "clientes",
                type: "text",
                nullable: false,
                defaultValue: "Activo");

            migrationBuilder.AddColumn<Guid>(
                name: "pais_id",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "region_id",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_ciudad_id",
                table: "clientes",
                column: "ciudad_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_estado",
                table: "clientes",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_pais_id",
                table: "clientes",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_region_id",
                table: "clientes",
                column: "region_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_clientes_estado",
                table: "clientes",
                sql: "estado IN ('Activo', 'Prospecto', 'Inactivo')");

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_ciudades_ciudad_id",
                table: "clientes",
                column: "ciudad_id",
                principalTable: "ciudades",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_paises_pais_id",
                table: "clientes",
                column: "pais_id",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_regiones_region_id",
                table: "clientes",
                column: "region_id",
                principalTable: "regiones",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_clientes_ciudades_ciudad_id",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "fk_clientes_paises_pais_id",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "fk_clientes_regiones_region_id",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "ix_clientes_ciudad_id",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "ix_clientes_estado",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "ix_clientes_pais_id",
                table: "clientes");

            migrationBuilder.DropIndex(
                name: "ix_clientes_region_id",
                table: "clientes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_clientes_estado",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "ciudad_id",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "pais_id",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "region_id",
                table: "clientes");
        }
    }
}
