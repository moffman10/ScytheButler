using Dashboard.ApiService.Services;
using ScytheButler.Data; // Ensure this matches your AppDbContext namespace

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// 1. Enable Controllers
builder.Services.AddControllers();

// 2. Register DbContext (Update options to match your DB provider, e.g., AddDbContextPool or UseSqlServer/UseNpgsql)
builder.Services.AddDbContext<AppDbContext>();

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

// 3. Map Controller Endpoints (replaces inline app.MapGet("/api/clan/{groupId:int}"))
app.MapControllers();

app.Run();