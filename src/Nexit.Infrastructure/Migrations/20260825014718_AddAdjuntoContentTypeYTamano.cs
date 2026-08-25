using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdjuntoContentTypeYTamano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "proveedor_adjuntos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tamano_bytes",
                table: "proveedor_adjuntos",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "proveedor_adjuntos");

            migrationBuilder.DropColumn(
                name: "tamano_bytes",
                table: "proveedor_adjuntos");
        }
    }
}
