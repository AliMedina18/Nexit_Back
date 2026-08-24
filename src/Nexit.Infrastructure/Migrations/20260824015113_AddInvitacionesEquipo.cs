using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitacionesEquipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitaciones_equipo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    invitado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_respuesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitaciones_equipo", x => x.id);
                    table.CheckConstraint("ck_invitaciones_equipo_estado", "estado IN ('Pendiente', 'Aceptada', 'Rechazada')");
                    table.ForeignKey(
                        name: "fk_invitaciones_equipo_usuarios_invitado_por_id",
                        column: x => x.invitado_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invitaciones_equipo_email_estado",
                table: "invitaciones_equipo",
                columns: new[] { "email", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_invitaciones_equipo_invitado_por_id",
                table: "invitaciones_equipo",
                column: "invitado_por_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitaciones_equipo");
        }
    }
}
