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

// Obtener cadena de conexión de Railway si existe, sino usar appsettings.json
var envHost = Environment.GetEnvironmentVariable("MYSQLHOST");
var envPort = Environment.GetEnvironmentVariable("MYSQLPORT");
var envDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
var envUser = Environment.GetEnvironmentVariable("MYSQLUSER");
var envPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");

string defaultConnection;

if (!string.IsNullOrEmpty(envHost) &&
    !string.IsNullOrEmpty(envPort) &&
    !string.IsNullOrEmpty(envDatabase) &&
    !string.IsNullOrEmpty(envUser) &&
    !string.IsNullOrEmpty(envPassword))
{
    defaultConnection = $"Server={envHost};Port={envPort};Database={envDatabase};User={envUser};Password={envPassword};";
}
else
{
    defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Configurar DbContext con MySQL y reintentos automáticos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        defaultConnection,
        new MySqlServerVersion(new Version(8, 0, 36)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly, typeof(UserProfile).Assembly);

// Registrar servicios y repositorios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // ✅ agregado

builder.Services.AddControllers();

// Configurar Swagger con JWT
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

var app = builder.Build();

// Escuchar en todas las interfaces (para Docker/Railway)
app.Urls.Add("http://0.0.0.0:8080");

// --- Swagger disponible siempre, incluso en Production ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "webProductos API v1");
    c.RoutePrefix = string.Empty; // swagger accesible en http://localhost:8080/
});

// Middleware
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

app.Run();
