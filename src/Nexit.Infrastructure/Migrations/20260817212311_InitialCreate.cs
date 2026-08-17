using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_proveedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias_proveedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dominios_correo_permitidos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dominio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dominios_correo_permitidos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fases_proyecto",
                columns: table => new
                {
                    fase = table.Column<short>(type: "smallint", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fases_proyecto", x => x.fase);
                });

            migrationBuilder.CreateTable(
                name: "paises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    etiqueta_region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_servicios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    apellido = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "text", nullable: false),
                    iniciales = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estados_proyecto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fase = table.Column<short>(type: "smallint", nullable: false),
                    orden = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estados_proyecto", x => x.id);
                    table.ForeignKey(
                        name: "fk_estados_proyecto_fases_proyecto_fase",
                        column: x => x.fase,
                        principalTable: "fases_proyecto",
                        principalColumn: "fase",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regiones", x => x.id);
                    table.ForeignKey(
                        name: "fk_regiones_paises_pais_id",
                        column: x => x.pais_id,
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sector = table.Column<string>(type: "text", nullable: true),
                    ciudad = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    web = table.Column<string>(type: "text", nullable: true),
                    contacto = table.Column<string>(type: "text", nullable: true),
                    cargo_contacto = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    valor_referencia = table.Column<string>(type: "text", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                    table.ForeignKey(
                        name: "fk_clientes_usuarios_created_by",
                        column: x => x.created_by,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "informes_snapshot",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    periodo_key = table.Column<string>(type: "text", nullable: false),
                    total_proveedores = table.Column<int>(type: "integer", nullable: false),
                    total_clientes = table.Column<int>(type: "integer", nullable: false),
                    total_proyectos = table.Column<int>(type: "integer", nullable: false),
                    proyectos_sin_proveedor = table.Column<int>(type: "integer", nullable: false),
                    por_estado = table.Column<string>(type: "jsonb", nullable: false),
                    por_brief = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_informes_snapshot", x => x.id);
                    table.ForeignKey(
                        name: "fk_informes_snapshot_usuarios_created_by",
                        column: x => x.created_by,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ciudades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ciudades", x => x.id);
                    table.ForeignKey(
                        name: "fk_ciudades_regiones_region_id",
                        column: x => x.region_id,
                        principalTable: "regiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cliente_telefonos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    etiqueta = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_telefonos", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_telefonos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyectos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contacto_proyecto = table.Column<string>(type: "text", nullable: true),
                    tipo_proyecto = table.Column<string>(type: "text", nullable: true),
                    prioridad = table.Column<string>(type: "text", nullable: true),
                    ciudad = table.Column<string>(type: "text", nullable: true),
                    sede_next = table.Column<string>(type: "text", nullable: true),
                    fecha_solicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    porcentaje_avance = table.Column<int>(type: "integer", nullable: false),
                    estado_brief = table.Column<string>(type: "text", nullable: false),
                    propuesta_estado = table.Column<string>(type: "text", nullable: false),
                    numero_factura = table.Column<string>(type: "text", nullable: true),
                    pagado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proyectos", x => x.id);
                    table.ForeignKey(
                        name: "fk_proyectos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_proyectos_estados_proyecto_estado_id",
                        column: x => x.estado_id,
                        principalTable: "estados_proyecto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proyectos_usuarios_created_by",
                        column: x => x.created_by,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ciudad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    contacto = table.Column<string>(type: "text", nullable: true),
                    cargo_contacto = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    web = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    aforo = table.Column<int>(type: "integer", nullable: true),
                    costo_referencia = table.Column<string>(type: "text", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: true),
                    presupuesto = table.Column<string>(type: "text", nullable: true),
                    cobertura = table.Column<string>(type: "text", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedores", x => x.id);
                    table.ForeignKey(
                        name: "fk_proveedores_categorias_proveedor_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias_proveedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proveedores_ciudades_ciudad_id",
                        column: x => x.ciudad_id,
                        principalTable: "ciudades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proveedores_paises_pais_id",
                        column: x => x.pais_id,
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proveedores_regiones_region_id",
                        column: x => x.region_id,
                        principalTable: "regiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proveedores_usuarios_created_by",
                        column: x => x.created_by,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "proyecto_equipo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proyecto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proyecto_equipo", x => x.id);
                    table.ForeignKey(
                        name: "fk_proyecto_equipo_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyecto_seguimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proyecto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    autor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    area = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nota = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proyecto_seguimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_proyecto_seguimiento_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_proyecto_seguimiento_usuarios_autor_id",
                        column: x => x.autor_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "proveedor_adjuntos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    url = table.Column<string>(type: "text", nullable: true),
                    storage_path = table.Column<string>(type: "text", nullable: true),
                    meta = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor_adjuntos", x => x.id);
                    table.ForeignKey(
                        name: "fk_proveedor_adjuntos_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proveedor_servicios",
                columns: table => new
                {
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    servicio_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor_servicios", x => new { x.proveedor_id, x.servicio_id });
                    table.ForeignKey(
                        name: "fk_proveedor_servicios_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_proveedor_servicios_servicios_servicio_id",
                        column: x => x.servicio_id,
                        principalTable: "servicios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proveedor_telefonos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    etiqueta = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor_telefonos", x => x.id);
                    table.ForeignKey(
                        name: "fk_proveedor_telefonos_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyecto_proveedores",
                columns: table => new
                {
                    proyecto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proyecto_proveedores", x => new { x.proyecto_id, x.proveedor_id });
                    table.ForeignKey(
                        name: "fk_proyecto_proveedores_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_proyecto_proveedores_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categorias_proveedor_nombre",
                table: "categorias_proveedor",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ciudades_region_id_nombre",
                table: "ciudades",
                columns: new[] { "region_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_telefonos_cliente_id",
                table: "cliente_telefonos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_ciudad",
                table: "clientes",
                column: "ciudad");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_created_by",
                table: "clientes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_email",
                table: "clientes",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_nombre",
                table: "clientes",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "ix_dominios_correo_permitidos_dominio",
                table: "dominios_correo_permitidos",
                column: "dominio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estados_proyecto_fase",
                table: "estados_proyecto",
                column: "fase");

            migrationBuilder.CreateIndex(
                name: "ix_estados_proyecto_nombre",
                table: "estados_proyecto",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estados_proyecto_orden",
                table: "estados_proyecto",
                column: "orden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fases_proyecto_nombre",
                table: "fases_proyecto",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_informes_snapshot_created_by",
                table: "informes_snapshot",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_informes_snapshot_tipo_periodo_key",
                table: "informes_snapshot",
                columns: new[] { "tipo", "periodo_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paises_nombre",
                table: "paises",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_adjuntos_proveedor_id",
                table: "proveedor_adjuntos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_servicios_servicio_id",
                table: "proveedor_servicios",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_telefonos_proveedor_id",
                table: "proveedor_telefonos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_categoria_id",
                table: "proveedores",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_ciudad_id",
                table: "proveedores",
                column: "ciudad_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_created_by",
                table: "proveedores",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_estado",
                table: "proveedores",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_pais_id",
                table: "proveedores",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_region_id",
                table: "proveedores",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyecto_equipo_proyecto_id",
                table: "proyecto_equipo",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyecto_proveedores_proveedor_id",
                table: "proyecto_proveedores",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyecto_seguimiento_autor_id",
                table: "proyecto_seguimiento",
                column: "autor_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyecto_seguimiento_proyecto_id",
                table: "proyecto_seguimiento",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_cliente_id",
                table: "proyectos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_created_by",
                table: "proyectos",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_estado_brief",
                table: "proyectos",
                column: "estado_brief");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_estado_id",
                table: "proyectos",
                column: "estado_id");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_fecha_evento",
                table: "proyectos",
                column: "fecha_evento");

            migrationBuilder.CreateIndex(
                name: "ix_proyectos_prioridad",
                table: "proyectos",
                column: "prioridad");

            migrationBuilder.CreateIndex(
                name: "ix_regiones_pais_id_nombre",
                table: "regiones",
                columns: new[] { "pais_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_servicios_nombre",
                table: "servicios",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cliente_telefonos");

            migrationBuilder.DropTable(
                name: "dominios_correo_permitidos");

            migrationBuilder.DropTable(
                name: "informes_snapshot");

            migrationBuilder.DropTable(
                name: "proveedor_adjuntos");

            migrationBuilder.DropTable(
                name: "proveedor_servicios");

            migrationBuilder.DropTable(
                name: "proveedor_telefonos");

            migrationBuilder.DropTable(
                name: "proyecto_equipo");

            migrationBuilder.DropTable(
                name: "proyecto_proveedores");

            migrationBuilder.DropTable(
                name: "proyecto_seguimiento");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.DropTable(
                name: "proyectos");

            migrationBuilder.DropTable(
                name: "categorias_proveedor");

            migrationBuilder.DropTable(
                name: "ciudades");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "estados_proyecto");

            migrationBuilder.DropTable(
                name: "regiones");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "fases_proyecto");

            migrationBuilder.DropTable(
                name: "paises");
        }
    }
}
