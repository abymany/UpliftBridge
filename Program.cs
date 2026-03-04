using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using UpliftBridge.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Render: bind to the provided PORT (required for Render web services)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

// -------------------------
// DATA PROTECTION (PERSIST KEYS ON RENDER DISK IF PRESENT)
// -------------------------
// Without persisting keys, antiforgery/session cookies can break after redeploy/restart
// (e.g., "The antiforgery token could not be decrypted").
var dpKeysPath = "/var/data/dpkeys";
if (Directory.Exists("/var/data"))
{
    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
        .SetApplicationName("UpliftBridge");
}

// -------------------------
// DATABASE CONFIG
// -------------------------
var env = builder.Environment;
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

if (env.IsProduction())
{
    if (string.IsNullOrWhiteSpace(connString))
        throw new Exception("Missing ConnectionStrings:DefaultConnection in Production.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    // Dev: prefer Postgres if connection string exists, otherwise SQLite
    if (!string.IsNullOrWhiteSpace(connString))
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

var app = builder.Build();

// If behind a proxy (Render), trust forwarded headers
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

// -------------------------
// MIGRATIONS + SEED (CONTROLLED)
// -------------------------
// Default: NO migrations on boot in Production.
// To run migrations on Render: set env var RUN_MIGRATIONS=true for ONE deploy, then remove it.
var runMigrations = string.Equals(
    builder.Configuration["RUN_MIGRATIONS"],
    "true",
    StringComparison.OrdinalIgnoreCase
);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
        SeedData.Initialize(db);
    }
    else if (runMigrations)
    {
        db.Database.Migrate();
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();