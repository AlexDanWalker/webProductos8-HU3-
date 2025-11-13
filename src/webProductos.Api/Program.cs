using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webProductos.Application.Interfaces;
using webProductos.Application.Services;
using webProductos.Application.Profiles;
using webProductos.Infrastructure.Data;
using webProductos.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

//  Cargar variables del .env si existe
Env.Load();

//  Configuración de la cadena de conexión
string connectionString;

//  Variables para local / Docker
var envHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
var envPort = Environment.GetEnvironmentVariable("MYSQL_PORT");
var envDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
var envUser = Environment.GetEnvironmentVariable("MYSQL_USER");
var envPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

//  Variables Aiven
var aivenConnection = Environment.GetEnvironmentVariable("AIVEN_CONNECTION");

//  Definir sslMode según Aiven o local
string sslMode = !string.IsNullOrEmpty(aivenConnection) ? "Required" : "Preferred";

if (!string.IsNullOrEmpty(aivenConnection))
{
    // Conexión Aiven
    connectionString = aivenConnection;
}
else if (!string.IsNullOrEmpty(envHost) &&
         !string.IsNullOrEmpty(envPort) &&
         !string.IsNullOrEmpty(envDatabase) &&
         !string.IsNullOrEmpty(envUser) &&
         !string.IsNullOrEmpty(envPassword))
{
    // Conexión Docker/local con SSL definido dinámicamente
    connectionString = $"Server={envHost};Port={envPort};Database={envDatabase};User={envUser};Password={envPassword};SslMode={sslMode};";
}
else
{
    // Conexión local de desarrollo desde appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

//  Retry loop para asegurar que MySQL esté listo
var maxRetries = 10;
var delay = TimeSpan.FromSeconds(5);
var connected = false;

for (int i = 0; i < maxRetries; i++)
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        // Intentamos abrir conexión rápida
        using var tempContext = new AppDbContext(optionsBuilder.Options);
        tempContext.Database.CanConnect();
        
        connected = true;
        Console.WriteLine(" Conexión a MySQL exitosa.");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Intento {i + 1} fallido: {ex.Message}");
        await Task.Delay(delay);
    }
}

if (!connected)
{
    throw new Exception("No se pudo conectar a MySQL después de varios intentos.");
}

//  Registrar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

//  AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly, typeof(UserProfile).Assembly);

//  Servicios y repositorios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

//  Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "webProductos API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

//  JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

//  Construir app
var app = builder.Build();
app.Urls.Add("http://0.0.0.0:8080"); // Docker / Render

//  Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "webProductos API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//  Seed de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

//  Arrancar app
app.Run();
