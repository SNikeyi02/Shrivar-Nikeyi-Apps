using Microsoft.EntityFrameworkCore;
using Prog_POE.Data;
using Prog_POE.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF, 2025.Quick start | QuestPDF. 
//code(Version 1.0)
//available at:
// <https://www.questpdf.com/quick-start.html>
//[Accessed 20 November 2025].
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
// Add SQL Server Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//witoldswitek 2025. WSZIB-shop/src/Shop.Web/Controllers/AccountController.cs 
//code(Version 1.0) [Source code].
//Available at:
// <https://github.com/witoldswitek/WSZIB-shop/tree/28044a663245ff30ba8bea87fd02a9ca176d8a29/src%2FShop.Web%2FControllers%2FAccountController.cs>
//[Accessed 20 November 2025].

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Create SecureUploads directory if it doesn't exist
var secureUploadsPath = Path.Combine(app.Environment.ContentRootPath, "SecureUploads");
if (!Directory.Exists(secureUploadsPath))
{
    Directory.CreateDirectory(secureUploadsPath);
}

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // Creates database if it doesn't exist

        // Seed initial data
        await SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");

        // Continue running the app even if seeding fails
    }
}

    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
