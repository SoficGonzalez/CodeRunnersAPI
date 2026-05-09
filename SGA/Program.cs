using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SGA.Configuration;
using SGA.Data;
using SGA.Middleware;
using SGA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SGA.Identity;

// Registrar code pages (Windows-1252, ISO-8859-1) que usa RtfTemplateParser.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.AddIdentity<AppIdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<SgaDbContext>()
    .AddDefaultTokenProviders();

var storageOptions = builder.Configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();
var maxBytes = storageOptions.TamanoMaximoBytes;

builder.Services.Configure<FormOptions>(opts =>
{
    opts.MultipartBodyLengthLimit = maxBytes;
    opts.ValueLengthLimit = int.MaxValue;
    opts.MemoryBufferThreshold = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.MaxRequestBodySize = maxBytes;
});

var connStr = builder.Configuration.GetConnectionString("SgaDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SgaDb' en la configuración.");

builder.Services.AddDbContext<SgaDbContext>(options =>
    options.UseSqlServer(connStr, sql => sql.EnableRetryOnFailure()));

// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IWordTemplateParser, WordTemplateParser>();
builder.Services.AddScoped<IRtfTemplateParser, RtfTemplateParser>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPlantillaService, PlantillaService>();
builder.Services.AddScoped<ISedeService, SedeService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SGA - Plantillas Word API",
        Version = "v1",
        Description = "API monolítica educativa para gestión de plantillas Word y llenado de documentos."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresa el token JWT."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGA API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
    var db = scope.ServiceProvider.GetRequiredService<SgaDbContext>();

    
    string[] rolesIdentity = { "Administrador", "RRHH", "Operador" };
    foreach (var role in rolesIdentity)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Creando admin por defecto
    if (await userManager.FindByEmailAsync("admin@sga.com") is null)
    {
        
        var rolAdmin = await db.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Administrador");
        if (rolAdmin is null)
        {
            rolAdmin = new SGA.Models.Rol { NombreRol = "Administrador", Activo = true };
            db.Roles.Add(rolAdmin);
            await db.SaveChangesAsync();
        }

        var usuarioAdmin = new SGA.Models.Usuario
        {
            NombreUsuario = "admin",
            NombreCompleto = "Administrador del Sistema",
            Correo = "admin@sga.com",
            RolId = rolAdmin.RolId,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        db.Usuarios.Add(usuarioAdmin);
        await db.SaveChangesAsync();

        var identityAdmin = new AppIdentityUser
        {
            UserName = "admin@sga.com",
            Email = "admin@sga.com",
            UsuarioId = usuarioAdmin.UsuarioId
        };

        await userManager.CreateAsync(identityAdmin, "Admin1234*");
        await userManager.AddToRoleAsync(identityAdmin, "Administrador");
    }
}

app.Run();

public partial class Program { }
