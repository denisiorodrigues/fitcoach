using System.Net;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Student;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class StudentsControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    [Fact]
    public async Task GetMyStudents_SemToken_Retorna401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyStudents_ComTokenDeAluno_Retorna403()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.GetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyStudents_RetornaSoAlunosDoTrainerAutenticado()
    {
        var trainerAClient = factory.CreateClient();
        var trainerA = await TestAuthHelper.RegisterTrainerAsync(trainerAClient);
        TestAuthHelper.Authenticate(trainerAClient, trainerA.Token);
        var alunoA = await TestAuthHelper.RegisterStudentAsync(trainerAClient);

        var trainerBClient = factory.CreateClient();
        var trainerB = await TestAuthHelper.RegisterTrainerAsync(trainerBClient);
        TestAuthHelper.Authenticate(trainerBClient, trainerB.Token);
        await TestAuthHelper.RegisterStudentAsync(trainerBClient);

        var response = await trainerAClient.GetAsync("/api/students");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var alunos = await response.Content.ReadFromJsonAsync<List<StudentProfileDto>>();
        alunos.Should().ContainSingle(a => a.User.Email == alunoA.Email);
    }

    [Fact]
    public async Task GetStudent_AlunoDeOutroTrainer_Retorna404()
    {
        var trainerAClient = factory.CreateClient();
        var trainerA = await TestAuthHelper.RegisterTrainerAsync(trainerAClient);
        TestAuthHelper.Authenticate(trainerAClient, trainerA.Token);

        var trainerBClient = factory.CreateClient();
        var trainerB = await TestAuthHelper.RegisterTrainerAsync(trainerBClient);
        TestAuthHelper.Authenticate(trainerBClient, trainerB.Token);
        var alunoB = await TestAuthHelper.RegisterStudentAsync(trainerBClient);

        var response = await trainerAClient.GetAsync($"/api/students/{alunoB.ProfileId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStudentActivity_AlunoDoTrainer_Retorna200()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var response = await trainerClient.GetAsync($"/api/students/{aluno.ProfileId}/activity");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStudentActivity_AlunoDeOutroTrainer_Retorna404()
    {
        var trainerAClient = factory.CreateClient();
        var trainerA = await TestAuthHelper.RegisterTrainerAsync(trainerAClient);
        TestAuthHelper.Authenticate(trainerAClient, trainerA.Token);

        var trainerBClient = factory.CreateClient();
        var trainerB = await TestAuthHelper.RegisterTrainerAsync(trainerBClient);
        TestAuthHelper.Authenticate(trainerBClient, trainerB.Token);
        var alunoB = await TestAuthHelper.RegisterStudentAsync(trainerBClient);

        var response = await trainerAClient.GetAsync($"/api/students/{alunoB.ProfileId}/activity");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
