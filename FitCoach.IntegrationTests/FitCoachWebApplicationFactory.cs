using FitCoach.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FitCoach.IntegrationTests;

public class FitCoachWebApplicationFactory : WebApplicationFactory<Program>
{
    // Gerado uma vez por factory (não dentro do lambda do AddDbContext, que reexecuta
    // a cada request porque DbContextOptions<T> é resolvido por escopo — cada request
    // acabava caindo num banco InMemory diferente).
    private readonly string _dbName = $"fitcoach-integration-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // AddDbContext acumula a configuração do Npgsql via IDbContextOptionsConfiguration<T>;
            // remover só o DbContextOptions<T> não é suficiente, senão os dois providers
            // (Npgsql + InMemory) ficam registrados ao mesmo tempo.
            services.RemoveAll<DbContextOptions<FitCoachDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FitCoachDbContext>>();

            services.AddDbContext<FitCoachDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}
