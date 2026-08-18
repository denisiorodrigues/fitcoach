using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Trainer;

namespace FitCoach.IntegrationTests;

public record AuthedUser(string Token, Guid UserId, Guid ProfileId, string Email);

public static class TestAuthHelper
{
    public static async Task<AuthedUser> RegisterTrainerAsync(HttpClient client, string? email = null)
    {
        email ??= $"trainer-{Guid.NewGuid():N}@teste.com";
        var req = new RegisterTrainerRequest("Trainer Teste", email, "senha123", "Musculação", "CREF-0001");

        var response = await client.PostAsJsonAsync("/api/auth/register/trainer", req);
        response.EnsureSuccessStatusCode();

        var body = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        return ToAuthedUser(body);
    }

    // trainerClient precisa já estar autenticado (ver Authenticate) com role Trainer.
    public static async Task<AuthedUser> RegisterStudentAsync(HttpClient trainerClient, string? email = null)
    {
        email ??= $"aluno-{Guid.NewGuid():N}@teste.com";
        var req = new RegisterStudentRequest("Aluno Teste", email, "senha123", "INVITE-CODE");

        var response = await trainerClient.PostAsJsonAsync("/api/auth/register/student", req);
        response.EnsureSuccessStatusCode();

        var body = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        return ToAuthedUser(body);
    }

    public static void Authenticate(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static AuthedUser ToAuthedUser(AuthResponse body)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(body.Token);
        var profileId = Guid.Parse(jwt.Claims.First(c => c.Type == "profileId").Value);
        return new AuthedUser(body.Token, body.User.Id, profileId, body.User.Email);
    }
}
