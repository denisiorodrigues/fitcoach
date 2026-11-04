using System.Net;

namespace FitCoach.Api.IntegrationTests;

/// <summary>
/// Classe de teste para verificar o endpoint de saúde da API FitCoach.
/// </summary>
public sealed class HealthEndpointTests : IClassFixture<FitCoachApiFactory>
{
    private readonly FitCoachApiFactory _factory;

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="HealthEndpointTests"/>.
    /// </summary>
    /// <param name="factory">A fábrica de aplicativo para criar uma instância do aplicativo FitCoach.Api para testes de integração.</param>
    public HealthEndpointTests(FitCoachApiFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Testa se o endpoint de saúde retorna um status HTTP 200 OK.
    /// </summary>
    /// <returns>Uma tarefa que representa a execução assíncrona do teste.</returns>
    [Fact]
    public async Task Get_Health_Returns_Ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
    }

    /// <summary>
    /// Testa se o endpoint de saúde retorna um corpo de resposta que contém a palavra "Healthy".
    /// </summary>
    /// <returns>Uma tarefa que representa a execução assíncrona do teste.</returns>
    [Fact]
    public async Task Get_Health_Returns_Healthy_Body()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contentBody = await response.Content.ReadAsStringAsync();

        Assert.Contains("Healthy", contentBody);
    }
}
