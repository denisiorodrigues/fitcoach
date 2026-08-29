using System.Net;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class DashboardControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    [Fact]
    public async Task GetDashboard_ComRoleTrainer_Retorna403()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);

        var response = await trainerClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDashboard_ComRoleStudent_Retorna200()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
