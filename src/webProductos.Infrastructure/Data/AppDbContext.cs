using Microsoft.EntityFrameworkCore;
using webProductos.Domain.Entities;

namespace webProductos.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Relaciones
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Configuración opcional (longitud, requeridos)
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).IsRequired().HasMaxLength(30);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).IsRequired().HasMaxLength(80);
                entity.Property(p => p.Description).HasMaxLength(255);
                entity.Property(p => p.Price).HasPrecision(10, 2);
            });
        }
    }
}