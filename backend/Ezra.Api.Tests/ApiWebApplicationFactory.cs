using Ezra.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ezra.Api.Tests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "ApiIntegrationTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            foreach (var d in services.Where(x => x.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList())
                services.Remove(d);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
