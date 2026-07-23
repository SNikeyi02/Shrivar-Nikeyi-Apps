using ERP.Interface;
using ERP.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add configuration to support test environment
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // 1. Add MVC Presentation Controllers & Views 
        builder.Services.AddControllersWithViews();

        builder.Services.AddHttpContextAccessor();

        string keysFolder = Path.Combine(Directory.GetCurrentDirectory(), "temp-keys-storage");
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
            .SetApplicationName("TechMoveLogisticsERP.Mvc");

        // 2. Configure the central HttpClient to communicate with the Web API Project
        // READ API BASE URL FROM CONFIGURATION (allows test environment override)
        var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7239/";

        builder.Services.AddHttpClient("ErpApiClient", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // 3. Keep standard Cookie Authentication for the UI Layer session
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "CookieAuth";
            options.DefaultAuthenticateScheme = "CookieAuth";
            options.DefaultChallengeScheme = "CookieAuth";
        })
        .AddCookie("CookieAuth", options =>
        {
            options.Cookie.Name = "TechMoveERP.Mvc.AdminAndClientAuthCookie";
            options.LoginPath = "/Home/Index";
            options.AccessDeniedPath = "/Home/Index";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Unspecified;
        });

        // 4. DESIGN PATTERN IMPLEMENTATION: Global Observer Registration
        builder.Services.AddSingleton<ContractStatusNotifier>(sp => {
            var notifier = new ContractStatusNotifier();
            notifier.Attach(new AdminNotificationObserver());
            return notifier;
        });

        var app = builder.Build();

        // 5. Middleware Pipeline Orchestration
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseMiddleware<ERP.Middleware.ExceptionHandlingMiddleware>();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}