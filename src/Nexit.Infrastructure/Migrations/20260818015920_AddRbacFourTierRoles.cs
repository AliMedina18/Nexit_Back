using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRbacFourTierRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios");

            migrationBuilder.AddColumn<Guid>(
                name: "gerente_id",
                table: "proyectos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "solicitudes_eliminacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tipo_entidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente_admin"),
                    gerente_responsable_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprobado_por_gerente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprobado_por_gerente_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revisado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revisado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    comentario_revision = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitudes_eliminacion", x => x.id);
                    table.CheckConstraint("ck_solicitudes_eliminacion_estado", "estado IN ('pendiente_gerente', 'pendiente_admin', 'aprobada', 'rechazada')");
                    table.CheckConstraint("ck_solicitudes_eliminacion_tipo", "tipo_entidad IN ('cliente', 'proveedor', 'proyecto')");
                    table.ForeignKey(
                        name: "fk_solicitudes_eliminacion_usuarios_aprobado_por_gerente_id",
                        column: x => x.aprobado_por_gerente_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_solicitudes_eliminacion_usuarios_gerente_responsable_id",
                        column: x => x.gerente_responsable_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_solicitudes_eliminacion_usuarios_revisado_por_id",
                        column: x => x.revisado_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_solicitudes_eliminacion_usuarios_solicitado_por_id",
                        column: x => x.solicitado_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios",
                sql: "rol IN ('super_admin', 'admin', 'manager', 'miembro')");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_gerente_id",
                table: "proyectos",
                column: "gerente_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_aprobado_por_gerente_id",
                table: "solicitudes_eliminacion",
                column: "aprobado_por_gerente_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_estado",
                table: "solicitudes_eliminacion",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_gerente_responsable_id",
                table: "solicitudes_eliminacion",
                column: "gerente_responsable_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_revisado_por_id",
                table: "solicitudes_eliminacion",
                column: "revisado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_solicitado_por_id",
                table: "solicitudes_eliminacion",
                column: "solicitado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_eliminacion_tipo_entidad_entidad_id",
                table: "solicitudes_eliminacion",
                columns: new[] { "tipo_entidad", "entidad_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_proyectos_usuarios_gerente_id",
                table: "proyectos",
                column: "gerente_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_proyectos_usuarios_gerente_id",
                table: "proyectos");

            migrationBuilder.DropTable(
                name: "solicitudes_eliminacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "ix_proyectos_gerente_id",
                table: "proyectos");

            migrationBuilder.DropColumn(
                name: "gerente_id",
                table: "proyectos");

            migrationBuilder.AddCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios",
                sql: "rol IN ('admin', 'manager', 'miembro')");
        }
    }
}
