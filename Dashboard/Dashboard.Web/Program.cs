using Dashboard.Web.Components;
using Dashboard.Web.Components.Pages.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ScytheButler.data.DatabaseModels.LoginModels;
using ScytheButler.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
});

// Configure authorization
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = false;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddOutputCache();

// Configures HttpClient to communicate with Dashboard.ApiService via Aspire service discovery
builder.Services.AddHttpClient("ApiService", client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
});

// Provides a typed/scoped HttpClient instance for direct injection into Razor components
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiService"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Map custom Resources folder to /Resources
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "Resources")),
    RequestPath = "/Resources"
});

app.UseAuthentication();

// Normal HTTP login endpoint.
// This must be a normal HTTP request because SignInManager
// needs to write the authentication cookie to the response.
app.MapPost("/account/login", async (
    HttpContext httpContext,
    SignInManager<ApplicationUser> signInManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();

    var username = form["Username"].ToString();
    var password = form["Password"].ToString();
    var rememberMe = form["RememberMe"].ToString() == "true";

    if (string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect("/login");
    }

    var result = await signInManager.PasswordSignInAsync(
        username,
        password,
        rememberMe,
        lockoutOnFailure: true);

    if (result.Succeeded)
    {
        return Results.Redirect("/");
    }

    if (result.IsLockedOut)
    {
        return Results.Redirect("/login?error=locked");
    }

    return Results.Redirect("/login?error=invalid");
});

app.MapStaticAssets()
    .Add(endpointBuilder =>
        endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));

app.UseAuthorization();

app.UseAntiforgery();
app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

