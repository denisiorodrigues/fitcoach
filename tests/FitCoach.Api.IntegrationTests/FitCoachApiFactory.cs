using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FitCoach.Api.IntegrationTests;

/// <summary>
/// Classe de fábrica personalizada para criar uma instância do aplicativo FitCoach.Api para testes de integração.
/// </summary>
public sealed class FitCoachApiFactory: WebApplicationFactory<Program>
{
    /// <summary>
    /// Configura o ambiente de teste e as configurações do aplicativo para os testes de integração.
    /// </summary>
    /// <param name="builder">Valor de configuração</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=FitCoachTestDb;Trusted_Connection=True;"
            });
        });
    }
}
