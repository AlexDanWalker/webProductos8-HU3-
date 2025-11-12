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
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>(); // ✅ Agregado

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Relación User ↔ Role (1:N)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación User ↔ Product (1:N)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Relación User ↔ RefreshToken (1:N)
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // 🔹 Configuración de Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).IsRequired().HasMaxLength(30);
            });

            // 🔹 Configuración de Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).IsRequired().HasMaxLength(80);
                entity.Property(p => p.Description).HasMaxLength(255);
                entity.Property(p => p.Price).HasPrecision(10, 2);
            });

            // 🔹 Configuración de RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.Property(rt => rt.Token).IsRequired();
                entity.Property(rt => rt.Created).IsRequired();
                entity.Property(rt => rt.Expires).IsRequired();
            });
        }
    }
}
