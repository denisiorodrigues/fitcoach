using System.Net;
using System.Net.Http.Json;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.DTOs.Workout;
using FluentAssertions;

namespace FitCoach.IntegrationTests;

public class WorkoutPlansControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    private async Task<(HttpClient trainerClient, AuthedUser trainer, AuthedUser aluno, Guid exercicioId)> SeedTrainerComAlunoEExercicioAsync()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var exResponse = await trainerClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Supino", "Chest", "Barbell", null, null, null));
        var exercicio = (await exResponse.Content.ReadFromJsonAsync<ExerciseDto>())!;

        return (trainerClient, trainer, aluno, exercicio.Id);
    }

    private static CreateWorkoutPlanRequest NovoPlanoRequest(Guid studentId, Guid exercicioId) => new(
        studentId, "Plano de Teste", null, null, null,
        [
            new CreateWorkoutDayRequest(DayOfWeek.Monday, "Treino A", null, 0,
                [
                    new CreatePlanExerciseRequest(exercicioId, 3, "10", 40, 60, 0, null)
                ])
        ]);

    [Fact]
    public async Task GetMyPlans_ComRoleStudent_Retorna403()
    {
        var (_, _, aluno, _) = await SeedTrainerComAlunoEExercicioAsync();
        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.GetAsync("/api/plans");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePlan_ComRoleStudent_Retorna403()
    {
        var (_, _, aluno, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();
        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(aluno.ProfileId, exercicioId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePlan_StudentNaoPertenceAoTrainer_Retorna400()
    {
        var (trainerClient, _, _, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();

        var response = await trainerClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(Guid.NewGuid(), exercicioId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePlanEGetPlan_TrainerDono_Retorna200()
    {
        var (trainerClient, _, aluno, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();

        var criado = await trainerClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(aluno.ProfileId, exercicioId));
        criado.StatusCode.Should().Be(HttpStatusCode.Created);
        var plano = (await criado.Content.ReadFromJsonAsync<WorkoutPlanDto>())!;

        var response = await trainerClient.GetAsync($"/api/plans/{plano.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlan_TrainerNaoDono_Retorna404()
    {
        var (trainerClient, _, aluno, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();
        var criado = await trainerClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(aluno.ProfileId, exercicioId));
        var plano = (await criado.Content.ReadFromJsonAsync<WorkoutPlanDto>())!;

        var outroTrainerClient = factory.CreateClient();
        var outroTrainer = await TestAuthHelper.RegisterTrainerAsync(outroTrainerClient);
        TestAuthHelper.Authenticate(outroTrainerClient, outroTrainer.Token);

        var response = await outroTrainerClient.GetAsync($"/api/plans/{plano.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlan_AlunoDono_Retorna200()
    {
        var (trainerClient, _, aluno, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();
        var criado = await trainerClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(aluno.ProfileId, exercicioId));
        var plano = (await criado.Content.ReadFromJsonAsync<WorkoutPlanDto>())!;

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.GetAsync($"/api/plans/{plano.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPlan_OutroAluno_Retorna404()
    {
        var (trainerClient, _, aluno, exercicioId) = await SeedTrainerComAlunoEExercicioAsync();
        var criado = await trainerClient.PostAsJsonAsync("/api/plans", NovoPlanoRequest(aluno.ProfileId, exercicioId));
        var plano = (await criado.Content.ReadFromJsonAsync<WorkoutPlanDto>())!;

        var outroAluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);
        var outroStudentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(outroStudentClient, outroAluno.Token);

        var response = await outroStudentClient.GetAsync($"/api/plans/{plano.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
