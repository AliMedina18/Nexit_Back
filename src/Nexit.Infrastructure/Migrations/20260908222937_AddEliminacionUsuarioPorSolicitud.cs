using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <summary>
    /// Lo que esta migración viene a hacer (docs/40):
    ///   1. <c>ck_solicitudes_eliminacion_tipo</c> acepta <c>'usuario'</c> -- eliminar a una persona
    ///      pasa por solicitud + decisión, ya no por DELETE /api/usuarios/{id}.
    ///   2. <c>ck_notificaciones_tipo</c> acepta <c>invitacion_aceptada</c> / <c>invitacion_rechazada</c>
    ///      -- estaban en el código desde el 2026-09-08 y la base los rechazaba al guardar.
    ///   3. Los tres FK que apuntaban a <c>usuarios</c> con RESTRICT (historial_cambios,
    ///      invitaciones_equipo, solicitudes_eliminacion) pasan a SET NULL, con sus columnas
    ///      nullables. Con RESTRICT era literalmente imposible eliminar a alguien que hubiera
    ///      editado, invitado o solicitado algo -- o sea, a cualquiera con uso real del sistema.
    ///
    /// Y algo más que arrastra sin haberlo pedido: el snapshot del modelo venía atrasado respecto a
    /// los scripts 20/21/22 de <c>docs/schema</c> (etapas_cliente, cliente_emails, proveedor_emails),
    /// que se aplicaron a mano en Supabase pero nunca como migración. Por eso el Up() también los
    /// crea. Eso está BIEN para una base local desde cero (<c>dotnet ef database update</c>) y está
    /// MAL para Supabase, donde esas tablas ya existen: allá no se corren migraciones, se corre
    /// <c>docs/schema/25_gestion_usuarios_al_dia.sql</c>, que trae solo los puntos 1-3 y es idempotente.
    /// </summary>
    public partial class AddEliminacionUsuarioPorSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_historial_cambios_usuarios_usuario_id",
                table: "historial_cambios");

            migrationBuilder.DropForeignKey(
                name: "fk_invitaciones_equipo_usuarios_invitado_por_id",
                table: "invitaciones_equipo");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitudes_eliminacion_usuarios_solicitado_por_id",
                table: "solicitudes_eliminacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_solicitudes_eliminacion_tipo",
                table: "solicitudes_eliminacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_notificaciones_tipo",
                table: "notificaciones");

            migrationBuilder.DropIndex(
                name: "ix_clientes_email",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "email",
                table: "proveedores");

            migrationBuilder.DropColumn(
                name: "email",
                table: "clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_por_id",
                table: "solicitudes_eliminacion",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "invitado_por_id",
                table: "invitaciones_equipo",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "usuario_id",
                table: "historial_cambios",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "etapa_id",
                table: "clientes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cliente_emails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    etiqueta = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_emails", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_emails_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "etapas_cliente",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false),
                    porcentaje_proceso = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etapas_cliente", x => x.id);
                    table.CheckConstraint("ck_etapas_cliente_porcentaje", "porcentaje_proceso BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "proveedor_emails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    etiqueta = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor_emails", x => x.id);
                    table.ForeignKey(
                        name: "fk_proveedor_emails_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_solicitudes_eliminacion_tipo",
                table: "solicitudes_eliminacion",
                sql: "tipo_entidad IN ('cliente', 'proveedor', 'proyecto', 'usuario')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_notificaciones_tipo",
                table: "notificaciones",
                sql: "tipo IN ('solicitud_eliminacion_creada', 'solicitud_eliminacion_endosada', 'solicitud_eliminacion_decidida', 'invitacion_aceptada', 'invitacion_rechazada')");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_etapa_id",
                table: "clientes",
                column: "etapa_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_emails_cliente_id",
                table: "cliente_emails",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_emails_email",
                table: "cliente_emails",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_etapas_cliente_nombre",
                table: "etapas_cliente",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_etapas_cliente_orden",
                table: "etapas_cliente",
                column: "orden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_emails_proveedor_id",
                table: "proveedor_emails",
                column: "proveedor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_clientes_etapas_cliente_etapa_id",
                table: "clientes",
                column: "etapa_id",
                principalTable: "etapas_cliente",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_historial_cambios_usuarios_usuario_id",
                table: "historial_cambios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_invitaciones_equipo_usuarios_invitado_por_id",
                table: "invitaciones_equipo",
                column: "invitado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitudes_eliminacion_usuarios_solicitado_por_id",
                table: "solicitudes_eliminacion",
                column: "solicitado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_clientes_etapas_cliente_etapa_id",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "fk_historial_cambios_usuarios_usuario_id",
                table: "historial_cambios");

            migrationBuilder.DropForeignKey(
                name: "fk_invitaciones_equipo_usuarios_invitado_por_id",
                table: "invitaciones_equipo");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitudes_eliminacion_usuarios_solicitado_por_id",
                table: "solicitudes_eliminacion");

            migrationBuilder.DropTable(
                name: "cliente_emails");

            migrationBuilder.DropTable(
                name: "etapas_cliente");

            migrationBuilder.DropTable(
                name: "proveedor_emails");

            migrationBuilder.DropCheckConstraint(
                name: "ck_solicitudes_eliminacion_tipo",
                table: "solicitudes_eliminacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_notificaciones_tipo",
                table: "notificaciones");

            migrationBuilder.DropIndex(
                name: "ix_clientes_etapa_id",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "etapa_id",
                table: "clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_por_id",
                table: "solicitudes_eliminacion",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "proveedores",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "invitado_por_id",
                table: "invitaciones_equipo",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "usuario_id",
                table: "historial_cambios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_solicitudes_eliminacion_tipo",
                table: "solicitudes_eliminacion",
                sql: "tipo_entidad IN ('cliente', 'proveedor', 'proyecto')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_notificaciones_tipo",
                table: "notificaciones",
                sql: "tipo IN ('solicitud_eliminacion_creada', 'solicitud_eliminacion_endosada', 'solicitud_eliminacion_decidida')");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_email",
                table: "clientes",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_historial_cambios_usuarios_usuario_id",
                table: "historial_cambios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_invitaciones_equipo_usuarios_invitado_por_id",
                table: "invitaciones_equipo",
                column: "invitado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitudes_eliminacion_usuarios_solicitado_por_id",
                table: "solicitudes_eliminacion",
                column: "solicitado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
