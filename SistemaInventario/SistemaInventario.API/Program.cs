using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaInventario.API.Data;
using SistemaInventario.API.Models;
using SistemaInventario.API.Middleware;
using SistemaInventario.API.Services;
using System.Text;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaCORS", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var clave = builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave!)
            ),

            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresá el token JWT. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IIngresoService, IngresoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ManejoErroresMiddleware>();
app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("PoliticaCORS");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.Migrate();

    // --- Seed de datos iniciales ---

    var admin = db.Usuarios.FirstOrDefault(u => u.Email == "admin@inventario.com");

    if (admin == null)
    {
        db.Usuarios.Add(new Usuario
        {
            Nombre = "Admin",
            Email = "admin@inventario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Rol = "Administrador"
        });
    }
    else if (!(admin.PasswordHash.Length == 60 && admin.PasswordHash.StartsWith("$2")))
    {
        // El hash guardado no es BCrypt válido: se repara
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        admin.Rol = "Administrador";
    }

    if (!db.Categorias.Any())
    {
        db.Categorias.AddRange(
            new Categoria { Nombre = "Electrónica", Descripcion = "Dispositivos electrónicos" },
            new Categoria { Nombre = "Alimentos", Descripcion = "Productos alimenticios" }
        );
    }

    if (!db.Proveedores.Any())
    {
        db.Proveedores.Add(new Proveedor
        {
            Nombre = "Proveedor Demo SA",
            Telefono = "3512345678",
            Email = "contacto@proveedordemo.com",
            Direccion = "Calle Falsa 123"
        });
    }
    if (!db.Usuarios.Any(u => u.Email == "cliente@inventario.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            Nombre = "Cliente Prueba",
            Email = "cliente@inventario.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
            Rol = "Usuario"
        });
    }

    db.SaveChanges();
    // Todo usuario tiene que tener su cliente asociado
    var usuariosSinCliente = db.Usuarios
        .Where(u => !db.Clientes.Any(c => c.UsuarioId == u.Id))
        .ToList();

    foreach (var u in usuariosSinCliente)
    {
        db.Clientes.Add(new Cliente
        {
            Nombre = u.Nombre,
            Apellido = "-",
            Email = u.Email,
            UsuarioId = u.Id
        });
    }

    db.SaveChanges();
    if (!db.Productos.Any())
    {
        var categoria = db.Categorias.First();

        db.Productos.AddRange(
            new Producto { Nombre = "Mouse Inalámbrico", Descripcion = "Mouse óptico", Precio = 5000, Stock = 0, CategoriaId = categoria.Id },
            new Producto { Nombre = "Teclado Mecánico", Descripcion = "Teclado retroiluminado", Precio = 15000, Stock = 0, CategoriaId = categoria.Id }
        );

        db.SaveChanges();
    }
}
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
