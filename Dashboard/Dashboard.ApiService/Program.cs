using Dashboard.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

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

// Wise Old Man Clan Endpoint using configured default Group ID
app.MapGet("/api/clan", async (IConfiguration config, WomService womService) =>
{
    var groupId = config.GetValue<int>("WiseOldMan:GroupId");

    if (groupId <= 0)
    {
        return Results.Problem("WiseOldMan:GroupId is not configured in appsettings.json.");
    }

    try
    {
        var group = await womService.GetGroupDetailsAsync(groupId);
        return group is not null ? Results.Ok(group) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("GetDefaultClanDetails");

// Optional: Keep parameter route if you ever need to fetch other groups dynamically
app.MapGet("/api/clan/{groupId:int}", async (int groupId, WomService womService) =>
{
    try
    {
        var group = await womService.GetGroupDetailsAsync(groupId);
        return group is not null ? Results.Ok(group) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("GetClanDetails");

app.MapDefaultEndpoints();

app.Run();