using ERP.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ERP.Tests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<ERP.Api.Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Set environment to Testing
            builder.UseEnvironment("Testing");
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {

                services.RemoveAll(typeof(DbContextOptions<GLMSDbContext>));

                services.AddDbContext<GLMSDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ERPTestDatabase");
                });

                services.RemoveAll(typeof(IHttpClientFactory));
                services.AddHttpClient("ErpApiClient", client =>
                {
                    client.BaseAddress = new Uri("http://localhost");
                });
            });
        }
    }
}