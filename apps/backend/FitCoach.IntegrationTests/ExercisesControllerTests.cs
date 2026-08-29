using System.Net;
using System.Net.Http.Json;
using FitCoach.API.Data;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FitCoach.IntegrationTests;

public class ExercisesControllerTests(FitCoachWebApplicationFactory factory) : IClassFixture<FitCoachWebApplicationFactory>
{
    // Não existe endpoint pra criar exercício global (CreateExercise sempre grava com o
    // TrainerId de quem chama); a única forma de ter um IsGlobal=true é semeando direto no banco.
    private async Task<Exercise> SeedExercicioGlobalAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FitCoachDbContext>();

        var exercise = new Exercise
        {
            TrainerId = Guid.NewGuid(),
            Name = $"Global {Guid.NewGuid():N}",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell,
            IsGlobal = true
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();
        return exercise;
    }

    [Fact]
    public async Task GetExercises_RetornaGlobaisMaisProprios_NaoRetornaDeOutroTrainer()
    {
        var global = await SeedExercicioGlobalAsync();

        var trainerAClient = factory.CreateClient();
        var trainerA = await TestAuthHelper.RegisterTrainerAsync(trainerAClient);
        TestAuthHelper.Authenticate(trainerAClient, trainerA.Token);
        (await trainerAClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Exercício A", "Chest", "Barbell", null, null, null))).EnsureSuccessStatusCode();

        var trainerBClient = factory.CreateClient();
        var trainerB = await TestAuthHelper.RegisterTrainerAsync(trainerBClient);
        TestAuthHelper.Authenticate(trainerBClient, trainerB.Token);
        await trainerBClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Exercício B", "Legs", "Machine", null, null, null));

        var response = await trainerAClient.GetAsync("/api/exercises");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var exercicios = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>();
        var nomes = exercicios!.Select(e => e.Name).ToList();
        nomes.Should().Contain(global.Name);
        nomes.Should().Contain("Exercício A");
        nomes.Should().NotContain("Exercício B");
    }

    [Fact]
    public async Task GetExercises_FiltraPorGrupoMuscular()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        await trainerClient.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest("Peito Filtro", "Chest", "Barbell", null, null, null));
        await trainerClient.PostAsJsonAsync("/api/exercises", new CreateExerciseRequest("Perna Filtro", "Legs", "Machine", null, null, null));

        var response = await trainerClient.GetAsync("/api/exercises?muscle=Chest");
        var exercicios = await response.Content.ReadFromJsonAsync<List<ExerciseDto>>();

        exercicios!.Should().OnlyContain(e => e.MuscleGroup == "Chest");
        exercicios.Select(e => e.Name).Should().Contain("Peito Filtro");
    }

    [Fact]
    public async Task PostExercise_ComRoleStudent_Retorna403()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);
        var aluno = await TestAuthHelper.RegisterStudentAsync(trainerClient);

        var studentClient = factory.CreateClient();
        TestAuthHelper.Authenticate(studentClient, aluno.Token);

        var response = await studentClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Tentativa", "Chest", "Barbell", null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostExercise_GrupoMuscularInvalido_Retorna400()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);

        var response = await trainerClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Inválido", "NaoExiste", "Barbell", null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostExercise_Valido_Retorna201ComTrainerIdDoToken()
    {
        var trainerClient = factory.CreateClient();
        var trainer = await TestAuthHelper.RegisterTrainerAsync(trainerClient);
        TestAuthHelper.Authenticate(trainerClient, trainer.Token);

        var response = await trainerClient.PostAsJsonAsync("/api/exercises",
            new CreateExerciseRequest("Supino Novo", "Chest", "Barbell", null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        body!.Name.Should().Be("Supino Novo");
        body.IsGlobal.Should().BeFalse();
    }
}
