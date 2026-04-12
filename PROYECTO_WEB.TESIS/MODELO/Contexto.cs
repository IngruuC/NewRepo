// MODELO/Contexto.cs — versión PostgreSQL
using ENTIDADES;
using ENTIDADES.SEGURIDAD;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace MODELO
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options) { }

        public Contexto() : base(GetDefaultOptions()) { }

        private static DbContextOptions<Contexto> GetDefaultOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Contexto>();
            optionsBuilder.UseNpgsql(
    "Host=ep-polished-sea-anqslb3e.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_OG5nUMQBlAK6;SslMode=Require;Trust Server Certificate=true");
            return optionsBuilder.Options;
        }

        // ── DbSets ──────────────────────────────────────────────────────────
        public DbSet<Usuario> Usuarios { get; set; }
       
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<AuditoriaSesion> AuditoriasSesion { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Compra> Compras { get; set; }

        public DbSet<LogActividad> LogActividad { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<EstadoUsuario> EstadosUsuario { get; set; }
        public DbSet<EstadoGrupo> EstadosGrupo { get; set; }
        public DbSet<Accion> Acciones { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Promocion> Promociones { get; set; }

        public DbSet<CatalogoProveedor> CatalogoProveedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── PRODUCTO ────────────────────────────────────────────────────
            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("numeric(18,2)");

            // ── USUARIO ─────────────────────────────────────────────────────
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario).IsUnique();
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.EstadoUsuario)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EstadoUsuarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ── GRUPO ────────────────────────────────────────────────────────
            modelBuilder.Entity<Grupo>().ToTable("Grupos");
            modelBuilder.Entity<Grupo>().HasKey(g => g.Id);
            modelBuilder.Entity<Grupo>()
                .Property(g => g.NombreGrupo).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Grupo>()
                .HasOne(g => g.EstadoGrupo)
                .WithMany(e => e.Grupos)
                .HasForeignKey(g => g.EstadoGrupoId)
                .IsRequired(false);

            // ── PERMISO ──────────────────────────────────────────────────────
            modelBuilder.Entity<Permiso>().ToTable("Permisos");
            modelBuilder.Entity<Permiso>().HasKey(p => p.Id);
            modelBuilder.Entity<Permiso>()
                .Property(p => p.NombrePermiso).IsRequired().HasMaxLength(50);

            // ── CLIENTE ──────────────────────────────────────────────────────
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<Cliente>().HasKey(c => c.Id);
            modelBuilder.Entity<Cliente>()
                .Property(c => c.Documento).IsRequired().HasMaxLength(8);
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Documento).IsUnique();
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .IsRequired(false);

            //PROMOCION
            modelBuilder.Entity<Promocion>(entity =>
            {
                entity.ToTable("Promocion");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoDescuento).IsRequired();
                entity.Property(e => e.ValorDescuento).HasColumnType("numeric(18,2)");
                entity.Property(e => e.Activa).HasDefaultValue(true);
                entity.Property(e => e.FechaInicio).HasDefaultValueSql("NOW()");
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId);
            });

            // ── PROVEEDOR ────────────────────────────────────────────────────
            modelBuilder.Entity<Proveedor>().ToTable("Proveedores");
            modelBuilder.Entity<Proveedor>().HasKey(p => p.Id);
            modelBuilder.Entity<Proveedor>()
                .Property(p => p.Cuit).IsRequired().HasMaxLength(11);
            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.Cuit).IsUnique();

            // ── VENTA ────────────────────────────────────────────────────────
            modelBuilder.Entity<Venta>().ToTable("Venta");
            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<Venta>()
                .Property(v => v.FechaVenta)
                .HasColumnName("Fecha");
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Venta>()
                .HasMany(v => v.Detalles)
                .WithOne(d => d.Venta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── DETALLE VENTA ────────────────────────────────────────────────
            modelBuilder.Entity<DetalleVenta>().ToTable("DetallesVenta");
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.ProductoNombre).HasMaxLength(100);
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            //FAVORITO
            modelBuilder.Entity<Favorito>(entity =>
            {
                entity.ToTable("Favorito");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.ClienteId);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId);
            });

            // ── COMPRA ────────────────────────────────────────────────────────
            modelBuilder.Entity<Compra>().ToTable("Compra");
            modelBuilder.Entity<Compra>()
                .Property(c => c.Total)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Compra>()
                .HasMany(c => c.Detalles)
                .WithOne(d => d.Compra)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── DETALLE COMPRA ────────────────────────────────────────────────
            modelBuilder.Entity<DetalleCompra>().ToTable("DetallesCompra");
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.Subtotal)
                .HasColumnType("numeric(18,2)");
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── ESTADO USUARIO ────────────────────────────────────────────────
            modelBuilder.Entity<EstadoUsuario>().ToTable("EstadosUsuario");
            modelBuilder.Entity<EstadoUsuario>().HasKey(e => e.Id);
            modelBuilder.Entity<EstadoUsuario>().HasData(
                new EstadoUsuario
                {
                    Id = 1,
                    Nombre = "Activo",
                    Descripcion = "Usuario activo por defecto"
                });
             
            // CATALOGO PROVEEDOR//
            modelBuilder.Entity<CatalogoProveedor>(entity =>
            {
                entity.ToTable("CatalogoProveedor");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreProducto).IsRequired();
                entity.Property(e => e.Precio).HasColumnType("numeric(18,2)");
                entity.Property(e => e.Estado).HasDefaultValue("Pendiente");
                entity.Property(e => e.FechaOferta).HasDefaultValueSql("NOW()");
                entity.HasOne(e => e.Proveedor)
                      .WithMany()
                      .HasForeignKey(e => e.ProveedorId);
            });

            // ── ESTADO GRUPO ──────────────────────────────────────────────────
            modelBuilder.Entity<EstadoGrupo>().ToTable("EstadosGrupo");
            modelBuilder.Entity<EstadoGrupo>().HasKey(e => e.Id);

            // ── ACCION ────────────────────────────────────────────────────────
            modelBuilder.Entity<Accion>().ToTable("Acciones");
            modelBuilder.Entity<Accion>().HasKey(a => a.Id);

            // ── AUDITORIA ─────────────────────────────────────────────────────
            modelBuilder.Entity<AuditoriaSesion>().ToTable("AuditoriasSesion");

            // ── PROVEEDOR PRODUCTO ────────────────────────────────────────────
            modelBuilder.Entity<ProveedorProducto>().HasKey(pp => pp.Id);
            modelBuilder.Entity<ProveedorProducto>()
                .HasIndex(pp => new { pp.ProveedorId, pp.ProductoId }).IsUnique();

            // ── MUCHOS A MUCHOS: Usuario <-> Grupo ────────────────────────────
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Grupos)
                .WithMany(g => g.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuariosGrupos",
                    j => j.HasOne<Grupo>().WithMany().HasForeignKey("GrupoId"),
                    j => j.HasOne<Usuario>().WithMany().HasForeignKey("UsuarioId")
                );

            // ── MUCHOS A MUCHOS: Grupo <-> Permiso ────────────────────────────
            modelBuilder.Entity<Grupo>()
                .HasMany(g => g.Permisos)
                .WithMany(p => p.Grupos)
                .UsingEntity<Dictionary<string, object>>(
                    "GruposPermisos",
                    j => j.HasOne<Permiso>().WithMany().HasForeignKey("PermisoId"),
                    j => j.HasOne<Grupo>().WithMany().HasForeignKey("GrupoId")
                );

            // ── MUCHOS A MUCHOS: Grupo <-> Accion ────────────────────────────
            modelBuilder.Entity<Grupo>()
                .HasMany(g => g.Acciones)
                .WithMany(a => a.Grupos)
                .UsingEntity<Dictionary<string, object>>(
                    "GruposAcciones",
                    j => j.HasOne<Accion>().WithMany().HasForeignKey("AccionId"),
                    j => j.HasOne<Grupo>().WithMany().HasForeignKey("GrupoId")
                );
        }
    }
}