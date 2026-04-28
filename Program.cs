using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Stripe;
using UpliftBridge.Data;

var builder = WebApplication.CreateBuilder(args);

// Render port binding
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".UpliftBridge.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// Database
var env = builder.Environment;
var connString = builder.Configuration.GetConnectionString("DefaultConnection")?.Trim();

if (env.IsProduction())
{
    if (string.IsNullOrWhiteSpace(connString))
        throw new Exception("Missing ConnectionStrings:DefaultConnection in Production.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    if (!string.IsNullOrWhiteSpace(connString) &&
        connString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connString));
    }
    else
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=UpliftBridge.db"));
    }
}

// Data Protection
var dpKeysPath = "/var/data/dpkeys";
if (Directory.Exists("/var/data"))
{
    Directory.CreateDirectory(dpKeysPath);
    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
        .SetApplicationName("UpliftBridge");
}
else if (env.IsProduction())
{
    builder.Services
        .AddDataProtection()
        .PersistKeysToDbContext<AppDbContext>()
        .SetApplicationName("UpliftBridge");
}
else
{
    builder.Services
        .AddDataProtection()
        .SetApplicationName("UpliftBridge");
}

var app = builder.Build();

// Render proxy headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Stripe
var stripeKey = builder.Configuration["Stripe:SecretKey"];
if (app.Environment.IsProduction() && string.IsNullOrWhiteSpace(stripeKey))
    throw new Exception("Stripe:SecretKey is missing in Production.");

if (!string.IsNullOrWhiteSpace(stripeKey))
    StripeConfiguration.ApiKey = stripeKey;

// Migrations + seed
var runMigrations = string.Equals(
    Environment.GetEnvironmentVariable("RUN_MIGRATIONS"),
    "true",
    StringComparison.OrdinalIgnoreCase
);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment() || runMigrations)
    {
        try { db.Database.Migrate(); }
        catch (Exception ex) { Console.WriteLine("Migration skipped: " + ex.Message); }
    }

    if (app.Environment.IsDevelopment())
    {
        try { SeedData.Initialize(db); }
        catch (Exception ex) { Console.WriteLine("Seed skipped: " + ex.Message); }
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Normal wwwroot static files
app.UseStaticFiles();

// Persistent Render uploaded files
var persistentUploadsPath = "/var/data/uploads";
if (Directory.Exists("/var/data"))
{
    Directory.CreateDirectory(persistentUploadsPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(persistentUploadsPath),
        RequestPath = "/uploads"
    });
}

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();