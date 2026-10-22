using Microsoft.EntityFrameworkCore;
using SistemaInventario.API.Models;

namespace SistemaInventario.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Proveedor> Proveedores { get; set; }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Ingreso> Ingresos { get; set; }

        public DbSet<DetalleIngreso> DetallesIngreso { get; set; }

        public DbSet<Venta> Ventas { get; set; }

        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }

        public DbSet<LogError> LogsError { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.UsuarioId);

            // Ninguna eliminación borra datos en cascada
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}