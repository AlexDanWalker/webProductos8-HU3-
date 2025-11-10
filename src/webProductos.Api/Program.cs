using Microsoft.EntityFrameworkCore;
using webProductos.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 1. Configurar conexión a MySQL
// (más adelante, reemplazaremos este connection string por el del docker-compose)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)) // versión estable de MySQL
    )
);

// 🔹 2. Configurar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 3. Construir la aplicación
var app = builder.Build();

// 🔹 4. Configurar middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🔹 5. Ejecutar seeder automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    await DataSeeder.SeedAsync(context);
}

app.Run();