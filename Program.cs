using System;
using UpliftBridge.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

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

// Stripe
var stripeKey = builder.Configuration["Stripe:SecretKey"];

if (app.Environment.IsProduction() && string.IsNullOrWhiteSpace(stripeKey))
    throw new Exception("Stripe:SecretKey is missing in Production.");

if (!string.IsNullOrWhiteSpace(stripeKey))
    StripeConfiguration.ApiKey = stripeKey;

// Migrations + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Always apply migrations (local + production)
    db.Database.Migrate();

    // Seed ONLY in Development (your computer), never in Production
    if (app.Environment.IsDevelopment())
    {
        SeedData.Initialize(db);
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