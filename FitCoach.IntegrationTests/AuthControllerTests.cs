using System.Net;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Trainer;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class AuthControllerTests : IClassFixture<FitCoachWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(FitCoachWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_RegisterTrainer_DadosValidos_Retorna201()
    {
        var req = new RegisterTrainerRequest("João Silva", "joao@fitcoach.com", "senha@123", "Musculação", "CREF-1234");

        var response = await _client.PostAsJsonAsync("/api/auth/register/trainer", req);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrEmpty();
        body.User.Email.Should().Be("joao@fitcoach.com");
    }

    [Fact]
    public async Task POST_RegisterTrainer_EmailDuplicado_Retorna409()
    {
        var req = new RegisterTrainerRequest("Maria", "maria@fitcoach.com", "senha", "Funcional", "CREF-999");

        await _client.PostAsJsonAsync("/api/auth/register/trainer", req);
        var response = await _client.PostAsJsonAsync("/api/auth/register/trainer", req);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_Login_CredenciaisCorretas_Retorna200ComToken()
    {
        var registro = new RegisterTrainerRequest("Ana Lima", "ana@fitcoach.com", "senha@123", "Pilates", "CREF-555");
        await _client.PostAsJsonAsync("/api/auth/register/trainer", registro);

        var login = new LoginRequest("ana@fitcoach.com", "senha@123");
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_Login_SenhaErrada_Retorna401()
    {
        var registro = new RegisterTrainerRequest("Carlos", "carlos@fitcoach.com", "senha-certa", "Yoga", "CREF-777");
        await _client.PostAsJsonAsync("/api/auth/register/trainer", registro);

        var login = new LoginRequest("carlos@fitcoach.com", "senha-errada");
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_RegisterStudent_SemToken_Retorna401()
    {
        var req = new { Name = "Aluno", Email = "aluno@fitcoach.com", Password = "senha123" };

        var response = await _client.PostAsJsonAsync("/api/auth/register/student", req);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
