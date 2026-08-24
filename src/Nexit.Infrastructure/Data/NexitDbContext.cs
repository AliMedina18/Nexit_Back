using Microsoft.EntityFrameworkCore;
using Nexit.Core.Entities;

namespace Nexit.Infrastructure.Data;

public class NexitDbContext(DbContextOptions<NexitDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Region> Regiones => Set<Region>();
    public DbSet<Ciudad> Ciudades => Set<Ciudad>();
    public DbSet<CategoriaProveedor> CategoriasProveedor => Set<CategoriaProveedor>();
    public DbSet<FaseProyecto> FasesProyecto => Set<FaseProyecto>();
    public DbSet<EstadoProyecto> EstadosProyecto => Set<EstadoProyecto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClienteTelefono> ClienteTelefonos => Set<ClienteTelefono>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ProveedorTelefono> ProveedorTelefonos => Set<ProveedorTelefono>();
    public DbSet<ProveedorAdjunto> ProveedorAdjuntos => Set<ProveedorAdjunto>();
    public DbSet<DominioCorreoPermitido> DominiosCorreoPermitidos => Set<DominioCorreoPermitido>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<ProveedorServicio> ProveedorServicios => Set<ProveedorServicio>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<ProyectoEquipo> ProyectoEquipo => Set<ProyectoEquipo>();
    public DbSet<ProyectoProveedor> ProyectoProveedores => Set<ProyectoProveedor>();
    public DbSet<ProyectoSeguimiento> ProyectoSeguimientos => Set<ProyectoSeguimiento>();
    public DbSet<InformeSnapshot> InformesSnapshot => Set<InformeSnapshot>();
    public DbSet<SolicitudEliminacion> SolicitudesEliminacion => Set<SolicitudEliminacion>();
    public DbSet<UsuarioEliminado> UsuariosEliminados => Set<UsuarioEliminado>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<HistorialCambio> HistorialCambios => Set<HistorialCambio>();
    public DbSet<ProveedorColaborador> ProveedorColaboradores => Set<ProveedorColaborador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(x => x.Id);
            entity.Ignore(x => x.CreatedBy);
            entity.Ignore(x => x.UpdatedBy);
            entity.ToTable(t => t.HasCheckConstraint("ck_usuarios_rol", "rol IN ('super_admin', 'admin', 'manager', 'miembro')"));
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Rol).HasDefaultValue("miembro"); entity.Property(x => x.Activo).HasDefaultValue(true); entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()"); entity.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        });
        modelBuilder.Entity<UsuarioEliminado>(entity =>
        {
            entity.ToTable("usuarios_eliminados");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Apellido).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Rol).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FechaEliminacion).HasDefaultValueSql("now()");
            entity.HasIndex(x => x.UsuarioIdOriginal);
        });
        modelBuilder.Entity<Pais>(entity =>
        {
            entity.ToTable("paises"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => x.Nombre).IsUnique();
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); entity.Property(x => x.EtiquetaRegion).HasColumnName("etiqueta_region").HasMaxLength(100).IsRequired();
            entity.HasMany(x => x.Regiones).WithOne(x => x.Pais).HasForeignKey(x => x.PaisId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Region>(entity =>
        {
            entity.ToTable("regiones"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => new { x.PaisId, x.Nombre }).IsUnique();
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); entity.HasMany(x => x.Ciudades).WithOne(x => x.Region).HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Ciudad>(entity => { entity.ToTable("ciudades"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => new { x.RegionId, x.Nombre }).IsUnique(); entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); });
        modelBuilder.Entity<CategoriaProveedor>(entity => { entity.ToTable("categorias_proveedor"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => x.Nombre).IsUnique(); entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); });
        modelBuilder.Entity<FaseProyecto>(entity =>
        {
            entity.ToTable("fases_proyecto"); entity.HasKey(x => x.Fase); entity.HasIndex(x => x.Nombre).IsUnique(); entity.Property(x => x.Fase).HasColumnName("fase").ValueGeneratedNever(); entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired();
            entity.HasMany(x => x.Estados).WithOne(x => x.FaseProyecto).HasForeignKey(x => x.Fase).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EstadoProyecto>(entity => { entity.ToTable("estados_proyecto"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => x.Nombre).IsUnique(); entity.HasIndex(x => x.Orden).IsUnique(); entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); });
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(x => x.Email).IsUnique().HasFilter("email IS NOT NULL");
            entity.HasIndex(x => x.Nombre); entity.HasIndex(x => x.Ciudad);
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired();
            entity.HasMany(x => x.Telefonos).WithOne(x => x.Cliente).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Proyectos).WithOne(x => x.Cliente).HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Usuario>().WithMany(x => x.ClientesCreados).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            entity.Property<uint>("xmin").HasColumnName("xmin").ValueGeneratedOnAddOrUpdate().IsRowVersion();
        });
        modelBuilder.Entity<ClienteTelefono>(entity =>
        {
            entity.ToTable("cliente_telefonos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Ignore(x => x.CreatedAt); entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.CreatedBy); entity.Ignore(x => x.UpdatedBy);
            entity.Property(x => x.Telefono).HasMaxLength(50).IsRequired();
        });
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("proveedores", t =>
            {
                t.HasCheckConstraint("ck_proveedores_score", "score IS NULL OR score BETWEEN 1 AND 5");
                t.HasCheckConstraint("ck_proveedores_estado", "estado IN ('Activo', 'En evaluación', 'Pausado', 'Bloqueado')");
                t.HasCheckConstraint("ck_proveedores_presupuesto", "presupuesto IS NULL OR presupuesto IN ('$ Bajo (<20k)', '$$ Medio (20k–100k)', '$$$ Alto (100k–500k)', '$$$$ Premium (>500k)')");
                t.HasCheckConstraint("ck_proveedores_cobertura", "cobertura IS NULL OR cobertura IN ('Solo ciudad', 'Regional', 'Nacional', 'Internacional')");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Estado).HasDefaultValue("Activo"); entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()"); entity.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(x => x.Estado);
            entity.HasOne<Pais>().WithMany().HasForeignKey(x => x.PaisId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Region>().WithMany().HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Ciudad>().WithMany().HasForeignKey(x => x.CiudadId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CategoriaProveedor>().WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Telefonos).WithOne(x => x.Proveedor).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Servicios).WithOne(x => x.Proveedor).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Adjuntos).WithOne(x => x.Proveedor).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Proyectos).WithOne(x => x.Proveedor).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Usuario>().WithMany(x => x.ProveedoresCreados).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            entity.Property<uint>("xmin").HasColumnName("xmin").ValueGeneratedOnAddOrUpdate().IsRowVersion();
        });
        modelBuilder.Entity<ProveedorTelefono>(entity => { entity.ToTable("proveedor_telefonos"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.Ignore(x => x.CreatedAt); entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.CreatedBy); entity.Ignore(x => x.UpdatedBy); entity.Property(x => x.Telefono).HasMaxLength(50).IsRequired(); });
        modelBuilder.Entity<ProveedorAdjunto>(entity => { entity.ToTable("proveedor_adjuntos", t => t.HasCheckConstraint("ck_proveedor_adjuntos_tipo", "tipo IN ('link', 'file')")); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.Property(x => x.Tipo).HasMaxLength(10).IsRequired(); entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired(); entity.Property(x => x.Fecha).HasDefaultValueSql("CURRENT_DATE"); entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()"); });
        modelBuilder.Entity<DominioCorreoPermitido>(entity => { entity.ToTable("dominios_correo_permitidos"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => x.Dominio).IsUnique(); entity.Property(x => x.Dominio).HasMaxLength(255).IsRequired(); });
        modelBuilder.Entity<Servicio>(entity => { entity.ToTable("servicios"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.HasIndex(x => x.Nombre).IsUnique(); entity.Ignore(x => x.UpdatedBy); });
        modelBuilder.Entity<ProveedorServicio>(entity =>
        {
            entity.ToTable("proveedor_servicios");
            entity.HasKey(x => new { x.ProveedorId, x.ServicioId });
            entity.HasOne(x => x.Servicio).WithMany(x => x.Proveedores).HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.ToTable("proyectos", t =>
            {
                t.HasCheckConstraint("ck_proyectos_porcentaje", "porcentaje_avance BETWEEN 0 AND 100");
                t.HasCheckConstraint("ck_proyectos_tipo", "tipo_proyecto IS NULL OR tipo_proyecto IN ('Corporativo', 'Evento social')");
                t.HasCheckConstraint("ck_proyectos_prioridad", "prioridad IS NULL OR prioridad IN ('Alta', 'Media', 'Baja')");
                t.HasCheckConstraint("ck_proyectos_brief", "estado_brief IN ('Pendiente por enviar', 'Entregado, a espera de respuesta', 'Requiere ajustes', 'Aprobado')");
                t.HasCheckConstraint("ck_proyectos_propuesta", "propuesta_estado IN ('No enviada', 'En proceso', 'Enviada')");
                t.HasCheckConstraint("ck_proyectos_pago", "NOT pagado OR fecha_pago IS NOT NULL");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Nombre).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PorcentajeAvance).HasDefaultValue(0); entity.Property(x => x.EstadoBrief).HasDefaultValue("Pendiente por enviar"); entity.Property(x => x.PropuestaEstado).HasDefaultValue("No enviada"); entity.Property(x => x.Pagado).HasDefaultValue(false); entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()"); entity.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(x => x.FechaEvento); entity.HasIndex(x => x.EstadoId); entity.HasIndex(x => x.EstadoBrief); entity.HasIndex(x => x.Prioridad);
            entity.HasOne<EstadoProyecto>().WithMany().HasForeignKey(x => x.EstadoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Equipo).WithOne(x => x.Proyecto).HasForeignKey(x => x.ProyectoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Proveedores).WithOne(x => x.Proyecto).HasForeignKey(x => x.ProyectoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Seguimiento).WithOne(x => x.Proyecto).HasForeignKey(x => x.ProyectoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Usuario>().WithMany(x => x.ProyectosCreados).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Gerente).WithMany().HasForeignKey(x => x.GerenteId).OnDelete(DeleteBehavior.SetNull);
            entity.Property<uint>("xmin").HasColumnName("xmin").ValueGeneratedOnAddOrUpdate().IsRowVersion();
        });
        modelBuilder.Entity<ProyectoEquipo>(entity => { entity.ToTable("proyecto_equipo", t => t.HasCheckConstraint("ck_proyecto_equipo_rol", "rol IN ('Ejecutivo', 'Comercial', 'Administrativo', 'Diseñador 3D', 'Diseñador gráfico')")); entity.HasKey(x => x.Id); entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()"); entity.Ignore(x => x.CreatedAt); entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.CreatedBy); entity.Ignore(x => x.UpdatedBy); entity.Property(x => x.Rol).HasMaxLength(100).IsRequired(); });
        modelBuilder.Entity<ProyectoProveedor>(entity =>
        {
            entity.ToTable("proyecto_proveedores");
            entity.HasKey(x => new { x.ProyectoId, x.ProveedorId });
        });
        modelBuilder.Entity<ProyectoSeguimiento>(entity =>
        {
            entity.ToTable("proyecto_seguimiento", t => t.HasCheckConstraint("ck_proyecto_seguimiento_area", "area IN ('General', 'Creativo', 'Comercial', 'Administrativo')"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.CreatedBy); entity.Ignore(x => x.UpdatedBy);
            entity.Property(x => x.Area).HasDefaultValue("General"); entity.Property(x => x.Fecha).HasDefaultValueSql("CURRENT_DATE"); entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(x => x.Autor).WithMany(x => x.SeguimientosEscritos).HasForeignKey(x => x.AutorId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<InformeSnapshot>(entity =>
        {
            entity.ToTable("informes_snapshot", t => t.HasCheckConstraint("ck_informes_snapshot_tipo", "tipo IN ('semanal', 'mensual')"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.UpdatedBy);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne<Usuario>().WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Tipo, x.PeriodoKey }).IsUnique();
            entity.Property(x => x.PorEstado).HasColumnType("jsonb");
            entity.Property(x => x.PorBrief).HasColumnType("jsonb");
        });
        modelBuilder.Entity<SolicitudEliminacion>(entity =>
        {
            entity.ToTable("solicitudes_eliminacion", t =>
            {
                t.HasCheckConstraint("ck_solicitudes_eliminacion_tipo", "tipo_entidad IN ('cliente', 'proveedor', 'proyecto')");
                t.HasCheckConstraint("ck_solicitudes_eliminacion_estado", "estado IN ('pendiente_gerente', 'pendiente_admin', 'aprobada', 'rechazada')");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.TipoEntidad).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("pendiente_admin");
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            entity.Ignore(x => x.UpdatedAt); entity.Ignore(x => x.CreatedBy); entity.Ignore(x => x.UpdatedBy);
            entity.HasIndex(x => new { x.TipoEntidad, x.EntidadId });
            entity.HasIndex(x => x.Estado);
            entity.HasOne(x => x.SolicitadoPor).WithMany().HasForeignKey(x => x.SolicitadoPorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GerenteResponsable).WithMany().HasForeignKey(x => x.GerenteResponsableId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.AprobadoPorGerente).WithMany().HasForeignKey(x => x.AprobadoPorGerenteId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.RevisadoPor).WithMany().HasForeignKey(x => x.RevisadoPorId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("notificaciones", t => t.HasCheckConstraint("ck_notificaciones_tipo",
                "tipo IN ('solicitud_eliminacion_creada', 'solicitud_eliminacion_endosada', 'solicitud_eliminacion_decidida')"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Titulo).HasMaxLength(255).IsRequired();
            entity.Property(x => x.TipoEntidad).HasMaxLength(20);
            entity.Property(x => x.FechaCreacion).HasDefaultValueSql("now()");
            entity.HasIndex(x => new { x.UsuarioDestinatarioId, x.Leida });
            entity.HasOne(x => x.UsuarioDestinatario).WithMany().HasForeignKey(x => x.UsuarioDestinatarioId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<HistorialCambio>(entity =>
        {
            entity.ToTable("historial_cambios", t => t.HasCheckConstraint("ck_historial_cambios_tipo_entidad", "tipo_entidad IN ('proyecto', 'proveedor', 'cliente')"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.TipoEntidad).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Accion).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Campo).HasMaxLength(100);
            entity.Property(x => x.Fecha).HasDefaultValueSql("now()");
            entity.HasIndex(x => new { x.TipoEntidad, x.EntidadId, x.Fecha });
            entity.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProveedorColaborador>(entity =>
        {
            entity.ToTable("proveedor_colaboradores");
            entity.HasKey(x => new { x.ProveedorId, x.UsuarioId });
            entity.Property(x => x.FechaAgregado).HasDefaultValueSql("now()");
            entity.HasOne(x => x.Proveedor).WithMany(x => x.Colaboradores).HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
