using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionesHistorialColaboradores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_desactivacion",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "historial_cambios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tipo_entidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valor_anterior = table.Column<string>(type: "text", nullable: true),
                    valor_nuevo = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_cambios", x => x.id);
                    table.CheckConstraint("ck_historial_cambios_tipo_entidad", "tipo_entidad IN ('proyecto', 'proveedor', 'cliente')");
                    table.ForeignKey(
                        name: "fk_historial_cambios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_destinatario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    tipo_entidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    solicitud_id = table.Column<Guid>(type: "uuid", nullable: true),
                    leida = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    fecha_leida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificaciones", x => x.id);
                    table.CheckConstraint("ck_notificaciones_tipo", "tipo IN ('solicitud_eliminacion_creada', 'solicitud_eliminacion_endosada', 'solicitud_eliminacion_decidida')");
                    table.ForeignKey(
                        name: "fk_notificaciones_usuarios_usuario_destinatario_id",
                        column: x => x.usuario_destinatario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proveedor_colaboradores",
                columns: table => new
                {
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_agregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor_colaboradores", x => new { x.proveedor_id, x.usuario_id });
                    table.ForeignKey(
                        name: "fk_proveedor_colaboradores_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_proveedor_colaboradores_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_eliminados",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    usuario_id_original = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    apellido = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    iniciales = table.Column<string>(type: "text", nullable: true),
                    fecha_alta_original = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_desactivacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    eliminado_por_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios_eliminados", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_historial_cambios_tipo_entidad_entidad_id_fecha",
                table: "historial_cambios",
                columns: new[] { "tipo_entidad", "entidad_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_cambios_usuario_id",
                table: "historial_cambios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificaciones_usuario_destinatario_id_leida",
                table: "notificaciones",
                columns: new[] { "usuario_destinatario_id", "leida" });

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_colaboradores_usuario_id",
                table: "proveedor_colaboradores",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_eliminados_usuario_id_original",
                table: "usuarios_eliminados",
                column: "usuario_id_original");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_cambios");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropTable(
                name: "proveedor_colaboradores");

            migrationBuilder.DropTable(
                name: "usuarios_eliminados");

            migrationBuilder.DropColumn(
                name: "fecha_desactivacion",
                table: "usuarios");
        }
    }
}
