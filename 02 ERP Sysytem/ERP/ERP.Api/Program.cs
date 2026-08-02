using ERP.Data;
using ERP.Enums;
using ERP.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from multiple sources for test flexibility
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 1. DB Context Connection Setup with test environment support
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Allow in-memory database for integration tests
if (builder.Environment.EnvironmentName == "Testing")
{
    builder.Services.AddDbContext<GLMSDbContext>(options =>
        options.UseInMemoryDatabase("ERPTestDatabase"));
}
else
{
    builder.Services.AddDbContext<GLMSDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// 2. Identity Setup on API Layer
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier;
    // Relax password requirements for testing
    if (builder.Environment.EnvironmentName == "Testing")
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    }
})
.AddEntityFrameworkStores<GLMSDbContext>()
.AddDefaultTokenProviders();

// Disable standard identity cookie generation on the API layer 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return System.Threading.Tasks.Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return System.Threading.Tasks.Task.CompletedTask;
    };
});

// 3. Configure JWT Validation Engine safely for local development
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "YourFallbackSuperSecretSystemKeyString2026");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };

    // Allow token from non-HTTPS in test environment
    if (builder.Environment.EnvironmentName == "Testing")
    {
        options.RequireHttpsMetadata = false;
    }
});

// 4. Add Controllers with custom JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Allow HTTP in test environment
if (app.Environment.EnvironmentName != "Testing")
{
    app.UseHttpsRedirection();
}

// Ensures the wwwroot directory exists and configures static file serving
string wwwrootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(wwwrootPath),
    RequestPath = ""
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed test data if in testing environment
if (app.Environment.EnvironmentName == "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GLMSDbContext>();
        SeedTestData(dbContext);
    }
}

app.Run();

// Helper method to seed test data
void SeedTestData(GLMSDbContext dbContext)
{
    // Add test client if not exists
    if (!dbContext.Clients.Any(c => c.Email == "testclient@client.com"))
    {
        var testClient = new Client
        {
            ClientID = Guid.NewGuid(),
            Name = "Test Client Company",
            Email = "testclient@client.com",
            Description = "Integration test client",
            Region = "Test Region",
            Contracts = new List<Contract>()
        };
        dbContext.Clients.Add(testClient);

        // Add a test contract
        var testContract = new Contract
        {
            ContractId = Guid.NewGuid(),
            ClientId = testClient.ClientID,
            Client = testClient,
            ServiceLevel = "Standard",
            Status = ContractStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            AgreementFilePath = "uploads/test.pdf"
        };
        dbContext.Contracts.Add(testContract);

        dbContext.SaveChanges();
    }
}

namespace ERP.Api
{
    public partial class Program { }
}