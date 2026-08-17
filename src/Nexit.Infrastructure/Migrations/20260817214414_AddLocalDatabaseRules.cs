using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalDatabaseRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "rol",
                table: "usuarios",
                type: "text",
                nullable: false,
                defaultValue: "miembro",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "activo",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "proyectos",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "propuesta_estado",
                table: "proyectos",
                type: "text",
                nullable: false,
                defaultValue: "No enviada",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "porcentaje_avance",
                table: "proyectos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "pagado",
                table: "proyectos",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "estado_brief",
                table: "proyectos",
                type: "text",
                nullable: false,
                defaultValue: "Pendiente por enviar",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proyectos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "proyecto_seguimiento",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proyecto_seguimiento",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "area",
                table: "proyecto_seguimiento",
                type: "text",
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "proveedores",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "proveedores",
                type: "text",
                nullable: false,
                defaultValue: "Activo",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proveedores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "proveedor_adjuntos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proveedor_adjuntos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "informes_snapshot",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios",
                sql: "rol IN ('admin', 'manager', 'miembro')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_brief",
                table: "proyectos",
                sql: "estado_brief IN ('Pendiente por enviar', 'Entregado, a espera de respuesta', 'Requiere ajustes', 'Aprobado')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_pago",
                table: "proyectos",
                sql: "NOT pagado OR fecha_pago IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_porcentaje",
                table: "proyectos",
                sql: "porcentaje_avance BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_prioridad",
                table: "proyectos",
                sql: "prioridad IS NULL OR prioridad IN ('Alta', 'Media', 'Baja')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_propuesta",
                table: "proyectos",
                sql: "propuesta_estado IN ('No enviada', 'En proceso', 'Enviada')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyectos_tipo",
                table: "proyectos",
                sql: "tipo_proyecto IS NULL OR tipo_proyecto IN ('Corporativo', 'Evento social')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyecto_seguimiento_area",
                table: "proyecto_seguimiento",
                sql: "area IN ('General', 'Creativo', 'Comercial', 'Administrativo')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proyecto_equipo_rol",
                table: "proyecto_equipo",
                sql: "rol IN ('Ejecutivo', 'Comercial', 'Administrativo', 'Diseñador 3D', 'Diseñador gráfico')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proveedores_cobertura",
                table: "proveedores",
                sql: "cobertura IS NULL OR cobertura IN ('Solo ciudad', 'Regional', 'Nacional', 'Internacional')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proveedores_estado",
                table: "proveedores",
                sql: "estado IN ('Activo', 'En evaluación', 'Pausado', 'Bloqueado')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proveedores_presupuesto",
                table: "proveedores",
                sql: "presupuesto IS NULL OR presupuesto IN ('$ Bajo (<20k)', '$$ Medio (20k–100k)', '$$$ Alto (100k–500k)', '$$$$ Premium (>500k)')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proveedores_score",
                table: "proveedores",
                sql: "score IS NULL OR score BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "ck_proveedor_adjuntos_tipo",
                table: "proveedor_adjuntos",
                sql: "tipo IN ('link', 'file')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_informes_snapshot_tipo",
                table: "informes_snapshot",
                sql: "tipo IN ('semanal', 'mensual')");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_updated_at()
                RETURNS trigger AS $$
                BEGIN
                  NEW.updated_at = now();
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION check_proveedor_geografia()
                RETURNS trigger AS $$
                DECLARE region_pais_id uuid; ciudad_region_id uuid;
                BEGIN
                  IF NEW.region_id IS NOT NULL THEN
                    SELECT pais_id INTO region_pais_id FROM regiones WHERE id = NEW.region_id;
                    IF region_pais_id IS DISTINCT FROM NEW.pais_id THEN RAISE EXCEPTION 'La región no pertenece al país indicado.'; END IF;
                  END IF;
                  IF NEW.ciudad_id IS NOT NULL THEN
                    SELECT region_id INTO ciudad_region_id FROM ciudades WHERE id = NEW.ciudad_id;
                    IF NEW.region_id IS NULL OR ciudad_region_id IS DISTINCT FROM NEW.region_id THEN RAISE EXCEPTION 'La ciudad no pertenece a la región indicada.'; END IF;
                  END IF;
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION check_usuario_dominio_correo()
                RETURNS trigger AS $$
                BEGIN
                  IF NOT EXISTS (SELECT 1 FROM dominios_correo_permitidos d WHERE lower(NEW.email) LIKE '%@' || lower(d.dominio)) THEN
                    RAISE EXCEPTION 'El correo no pertenece a un dominio laboral permitido.';
                  END IF;
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION set_estado_proyecto_default()
                RETURNS trigger AS $$
                BEGIN
                  IF NEW.estado_id IS NULL THEN
                    SELECT id INTO NEW.estado_id FROM estados_proyecto WHERE nombre = 'Planeación interna';
                  END IF;
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_clientes_updated_at BEFORE UPDATE ON clientes FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                CREATE TRIGGER trg_proveedores_updated_at BEFORE UPDATE ON proveedores FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                CREATE TRIGGER trg_proyectos_updated_at BEFORE UPDATE ON proyectos FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                CREATE TRIGGER trg_usuarios_updated_at BEFORE UPDATE ON usuarios FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                CREATE TRIGGER trg_proveedores_geografia BEFORE INSERT OR UPDATE ON proveedores FOR EACH ROW EXECUTE FUNCTION check_proveedor_geografia();
                CREATE TRIGGER trg_usuarios_dominio_correo BEFORE INSERT OR UPDATE OF email ON usuarios FOR EACH ROW EXECUTE FUNCTION check_usuario_dominio_correo();
                CREATE TRIGGER trg_proyectos_estado_default BEFORE INSERT ON proyectos FOR EACH ROW EXECUTE FUNCTION set_estado_proyecto_default();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_clientes_updated_at ON clientes;
                DROP TRIGGER IF EXISTS trg_proveedores_updated_at ON proveedores;
                DROP TRIGGER IF EXISTS trg_proyectos_updated_at ON proyectos;
                DROP TRIGGER IF EXISTS trg_usuarios_updated_at ON usuarios;
                DROP TRIGGER IF EXISTS trg_proveedores_geografia ON proveedores;
                DROP TRIGGER IF EXISTS trg_usuarios_dominio_correo ON usuarios;
                DROP TRIGGER IF EXISTS trg_proyectos_estado_default ON proyectos;
                DROP FUNCTION IF EXISTS set_updated_at();
                DROP FUNCTION IF EXISTS check_proveedor_geografia();
                DROP FUNCTION IF EXISTS check_usuario_dominio_correo();
                DROP FUNCTION IF EXISTS set_estado_proyecto_default();
                """);
            migrationBuilder.DropCheckConstraint(
                name: "ck_usuarios_rol",
                table: "usuarios");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_brief",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_pago",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_porcentaje",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_prioridad",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_propuesta",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyectos_tipo",
                table: "proyectos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyecto_seguimiento_area",
                table: "proyecto_seguimiento");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proyecto_equipo_rol",
                table: "proyecto_equipo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proveedores_cobertura",
                table: "proveedores");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proveedores_estado",
                table: "proveedores");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proveedores_presupuesto",
                table: "proveedores");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proveedores_score",
                table: "proveedores");

            migrationBuilder.DropCheckConstraint(
                name: "ck_proveedor_adjuntos_tipo",
                table: "proveedor_adjuntos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_informes_snapshot_tipo",
                table: "informes_snapshot");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "rol",
                table: "usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "miembro");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<bool>(
                name: "activo",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "proyectos",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "propuesta_estado",
                table: "proyectos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "No enviada");

            migrationBuilder.AlterColumn<int>(
                name: "porcentaje_avance",
                table: "proyectos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "pagado",
                table: "proyectos",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "estado_brief",
                table: "proyectos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pendiente por enviar");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proyectos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "proyecto_seguimiento",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_DATE");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proyecto_seguimiento",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "area",
                table: "proyecto_seguimiento",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "General");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "proveedores",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "proveedores",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Activo");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proveedores",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha",
                table: "proveedor_adjuntos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_DATE");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "proveedor_adjuntos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "informes_snapshot",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
