using Microsoft.EntityFrameworkCore;
using webProductos.Domain.Entities;

namespace webProductos.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Aplicar migraciones pendientes automáticamente
            await context.Database.MigrateAsync();

            // Si ya hay roles, no sembrar nada
            if (context.Roles.Any()) return;

            //  Roles base
            var roles = new List<Role>
            {
                new Role { Name = "Admin" },
                new Role { Name = "Seller" },
                new Role { Name = "Customer" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            //  Usuarios de prueba (contraseñas: admin123, seller123, customer123)
            var users = new List<User>
            {
                new User
                {
                    Username = "adminUser",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = roles.First(r => r.Name == "Admin").Id
                },
                new User
                {
                    Username = "sellerUser",
                    Email = "seller@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("seller123"),
                    RoleId = roles.First(r => r.Name == "Seller").Id
                },
                new User
                {
                    Username = "customerUser",
                    Email = "customer@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                    RoleId = roles.First(r => r.Name == "Customer").Id
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            //  Productos de prueba
            var products = new List<Product>
            {
                new Product { Name = "Laptop Gamer", Description = "Laptop de alto rendimiento", Price = 4999.99m, UserId = users[1].Id },
                new Product { Name = "Mouse RGB", Description = "Mouse ergonómico con luces LED", Price = 59.99m, UserId = users[1].Id },
                new Product { Name = "Teclado Mecánico", Description = "Teclado con switches azules", Price = 120.00m, UserId = users[1].Id },
                new Product { Name = "Auriculares Inalámbricos", Description = "Bluetooth con cancelación de ruido", Price = 199.99m, UserId = users[2].Id }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
