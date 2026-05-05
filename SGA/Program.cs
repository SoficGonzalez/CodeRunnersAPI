using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SGA.Configuration;
using SGA.Data;
using SGA.Middleware;
using SGA.Services;

// Registrar code pages (Windows-1252, ISO-8859-1) que usa RtfTemplateParser.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

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

builder.Services.AddScoped<IWordTemplateParser, WordTemplateParser>();
builder.Services.AddScoped<IRtfTemplateParser, RtfTemplateParser>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPlantillaService, PlantillaService>();

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
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
