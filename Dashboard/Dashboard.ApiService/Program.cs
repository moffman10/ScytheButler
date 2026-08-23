using Dashboard.ApiService.Services;
using Microsoft.EntityFrameworkCore;
using ScytheButler.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// 1. Enable Controllers
builder.Services.AddControllers();

// 2. Fetch Connection String from Configuration (appsettings / User Secrets)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 3. Register DbContext with SQL Server Provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register WomService with its HTTP Client
builder.Services.AddHttpClient<WomService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Default root check
app.MapGet("/", () => "API service is running.");

app.MapDefaultEndpoints();

// Map Controller Endpoints
try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine($"LOADER EXCEPTION: {loaderException?.Message}");
    }
    throw;
}

app.Run();