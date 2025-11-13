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

var builder = WebApplication.CreateBuilder(args);

// Configuración de la cadena de conexión

// Obtener variables de entorno (Render / Aiven)
var envHost = Environment.GetEnvironmentVariable("MYSQLHOST");
var envPort = Environment.GetEnvironmentVariable("MYSQLPORT");
var envDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
var envUser = Environment.GetEnvironmentVariable("MYSQLUSER");
var envPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");

// Decidir qué conexión usar: Aiven (Render) o local
string connectionString;

if (!string.IsNullOrEmpty(envHost) &&
    !string.IsNullOrEmpty(envPort) &&
    !string.IsNullOrEmpty(envDatabase) &&
    !string.IsNullOrEmpty(envUser) &&
    !string.IsNullOrEmpty(envPassword))
{
    // Conexión Aiven con SSL requerido
    connectionString = $"Server={envHost};Port={envPort};Database={envDatabase};User={envUser};Password={envPassword};SslMode=Required;";
}
else
{
    // Conexión local para desarrollo
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Registrar DbContext con reintentos automáticos
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString), // detecta versión automáticamente
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    );
});

// Registrar AutoMapper

builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly, typeof(UserProfile).Assembly);

// Registrar servicios y repositorios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Configurar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "webProductos API", Version = "v1" });

    // JWT en Swagger
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar JWT
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

// Construir la app
var app = builder.Build();

// Escuchar en todas las interfaces (Docker / Render)
app.Urls.Add("http://0.0.0.0:8080");

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "webProductos API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}
// Arrancar la app
app.Run();
