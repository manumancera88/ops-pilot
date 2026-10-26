

using backend.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace backend.Tests.Infrastructure;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    "IntegrationTestKey_2026_AtLeast32Characters_Long",
                ["Jwt:Issuer"] = "OpsPilotTests",
                ["Jwt:Audience"] = "OpsPilotTests"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            // Remover os registros diretos do AppDbContext.
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            // Remover a configuração do provider SQL Server registrada
            // pelo Program.cs, sem depender de tipos internos do EF Core.
            var databaseConfigurations = services
                .Where(descriptor =>
                    descriptor.ServiceType.IsGenericType &&
                    descriptor.ServiceType.GetGenericTypeDefinition()
                        .FullName?
                        .StartsWith(
                            "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration",
                            StringComparison.Ordinal) == true &&
                    descriptor.ServiceType.GenericTypeArguments[0]
                        == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in databaseConfigurations)
            {
                services.Remove(descriptor);
            }

            // Registrar somente o provider InMemory para os testes.
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(
    "OpsPilotIntegrationTests");

                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(
                        InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }
}
