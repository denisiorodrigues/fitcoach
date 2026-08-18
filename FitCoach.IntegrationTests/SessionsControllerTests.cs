using System.Net;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.DTOs.Workout;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class SessionsControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    private async Task<(HttpClient studentClient, Guid workoutDayId, Guid planExerciseId)> SeedAlunoComTreinoAsync()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var exResponse = await trainerClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Supino", "Chest", "Barbell", null, null, null));
        var exercicio = (await exResponse.Content.ReadFromJsonAsync<ExerciseDto>())!;

        var planReq = new CreateWorkoutPlanRequest(aluno.ProfileId, "Plano", null, null, null,
            [
                new CreateWorkoutDayRequest(DayOfWeek.Monday, "Treino A", null, 0,
                    [
                        new CreatePlanExerciseRequest(exercicio.Id, 3, "10", 40, 60, 0, null)
                    ])
            ]);
        var planResponse = await trainerClient.PostAsJsonAsync("/api/plans", planReq);
        var plano = (await planResponse.Content.ReadFromJsonAsync<WorkoutPlanDto>())!;

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        return (studentClient, plano.Days[0].Id, plano.Days[0].Exercises[0].Id);
    }

    [Fact]
    public async Task StartSession_ComRoleTrainer_Retorna403()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);

        var response = await trainerClient.PostAsJsonAsync("/api/sessions/start", new StartSessionRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FluxoCompleto_StartLogFinishGet_FuncionaEReflete()
    {
        var (studentClient, workoutDayId, planExerciseId) = await SeedAlunoComTreinoAsync();

        var startResponse = await studentClient.PostAsJsonAsync("/api/sessions/start", new StartSessionRequest(workoutDayId));
        startResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var sessao = (await startResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>())!;

        var logResponse = await studentClient.PostAsJsonAsync($"/api/sessions/{sessao.Id}/sets",
            new LogSetRequest(planExerciseId, 1, 10, 42.5f));
        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var finishResponse = await studentClient.PostAsJsonAsync($"/api/sessions/{sessao.Id}/finish",
            new FinishSessionRequest(140, 300, "Bom treino"));
        finishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await studentClient.GetAsync($"/api/sessions/{sessao.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var final = (await getResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>())!;

        final.Sets.Should().ContainSingle();
        final.FinishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogSet_SessaoDeOutroAluno_Retorna404()
    {
        var (studentAClient, workoutDayId, planExerciseId) = await SeedAlunoComTreinoAsync();
        var startResponse = await studentAClient.PostAsJsonAsync("/api/sessions/start", new StartSessionRequest(workoutDayId));
        var sessao = (await startResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>())!;

        var (studentBClient, _, _) = await SeedAlunoComTreinoAsync();

        var response = await studentBClient.PostAsJsonAsync($"/api/sessions/{sessao.Id}/sets",
            new LogSetRequest(planExerciseId, 1, 10, 40));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSession_Inexistente_Retorna404()
    {
        var (studentClient, _, _) = await SeedAlunoComTreinoAsync();

        var response = await studentClient.GetAsync($"/api/sessions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
