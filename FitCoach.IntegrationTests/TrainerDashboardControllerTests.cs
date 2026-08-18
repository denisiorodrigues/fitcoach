using System.Net;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Trainer;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class TrainerDashboardControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    [Fact]
    public async Task GetDashboard_ComRoleStudent_Retorna403()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.GetAsync("/api/trainer/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDashboard_ComRoleTrainer_RetornaContadoresCorretos()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        await TestAuthHelper.RegisterStudentAsync(trainerClient);
        await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var response = await trainerClient.GetAsync("/api/trainer/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<TrainerDashboardDto>();
        dto!.TotalStudents.Should().Be(2);
    }
}
