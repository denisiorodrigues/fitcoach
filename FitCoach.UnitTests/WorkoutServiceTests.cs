using FitCoach.API.Data;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.DTOs.Workout;
using FitCoach.API.Models;
using FitCoach.API.Services;
using FitCoach.UnitTests.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitCoach.UnitTests;

public class WorkoutServiceTests : IDisposable
{
    private readonly FitCoachDbContext _db;
    private readonly WorkoutService _sut;

    public WorkoutServiceTests()
    {
        var options = new DbContextOptionsBuilder<FitCoachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new FitCoachDbContext(options);
        _sut = new WorkoutService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private async Task<(TrainerProfile trainer, StudentProfile student)> SeedTrainerComAlunoAsync()
    {
        var trainerUser = UserFaker.Default().Generate();
        var studentUser = UserFaker.Default().RuleFor(u => u.Role, UserRole.Student).Generate();
        _db.Users.AddRange(trainerUser, studentUser);
        await _db.SaveChangesAsync();

        var trainer = TreinerFake.Default(trainerUser.Id).Generate();
        _db.TrainerProfiles.Add(trainer);
        await _db.SaveChangesAsync();

        var student = new StudentProfile
        {
            UserId = studentUser.Id,
            TrainerId = trainer.Id,
            EnrolledAt = DateTime.UtcNow
        };
        _db.StudentProfiles.Add(student);
        await _db.SaveChangesAsync();

        return (trainer, student);
    }

    private async Task<(WorkoutPlan plan, WorkoutDay day, PlanExercise planExercise)> SeedPlanComExercicioAsync(
        Guid trainerId, Guid studentId, DayOfWeek dayOfWeek, bool isActive = true, DateTime? createdAt = null)
    {
        var exercise = new Exercise
        {
            TrainerId = trainerId,
            Name = "Supino Reto",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };

        var planExercise = new PlanExercise
        {
            Exercise = exercise,
            Sets = 3,
            Reps = "10",
            WeightKg = 40,
            RestSeconds = 60,
            OrderIndex = 0
        };

        var day = new WorkoutDay
        {
            DayOfWeek = dayOfWeek,
            Label = "Treino A",
            OrderIndex = 0,
            Exercises = new List<PlanExercise> { planExercise }
        };

        var plan = new WorkoutPlan
        {
            TrainerId = trainerId,
            StudentId = studentId,
            Name = "Plano A",
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Days = new List<WorkoutDay> { day }
        };

        _db.WorkoutPlans.Add(plan);
        await _db.SaveChangesAsync();

        return (plan, day, planExercise);
    }

    // ─── GetTrainerPlansAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetTrainerPlansAsync_RetornaSoPlanosDoTrainer()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        var (outroTrainer, outroStudent) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(outroTrainer.Id, outroStudent.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.GetTrainerPlansAsync(trainer.Id);

        //Assert
        resultado.Should().ContainSingle();
        resultado[0].StudentId.Should().Be(student.Id);
    }

    [Fact]
    public async Task GetTrainerPlansAsync_OrdenaPorCreatedAtDecrescente()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday, createdAt: DateTime.UtcNow.AddDays(-5));
        var (planoRecente, _, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Tuesday, createdAt: DateTime.UtcNow);

        //Act
        var resultado = await _sut.GetTrainerPlansAsync(trainer.Id);

        //Assert
        resultado.Should().HaveCount(2);
        resultado[0].Id.Should().Be(planoRecente.Id);
    }

    // ─── GetPlanDetailAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetPlanDetailAsync_PlanoInexistente_RetornaNull()
    {
        //Act
        var resultado = await _sut.GetPlanDetailAsync(Guid.NewGuid(), Guid.NewGuid(), isTrainer: true);

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanDetailAsync_TrainerDono_RetornaPlano()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (plan, _, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.GetPlanDetailAsync(plan.Id, trainer.Id, isTrainer: true);

        //Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(plan.Id);
    }

    [Fact]
    public async Task GetPlanDetailAsync_TrainerNaoDono_RetornaNull()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (plan, _, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.GetPlanDetailAsync(plan.Id, Guid.NewGuid(), isTrainer: true);

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetPlanDetailAsync_AlunoDono_RetornaPlano()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (plan, _, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.GetPlanDetailAsync(plan.Id, student.Id, isTrainer: false);

        //Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPlanDetailAsync_AlunoNaoDono_RetornaNull()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (plan, _, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.GetPlanDetailAsync(plan.Id, Guid.NewGuid(), isTrainer: false);

        //Assert
        resultado.Should().BeNull();
    }

    // ─── CreatePlanAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePlanAsync_StudentDoTrainer_CriaPlanoComDiasEExercicios()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var exercise = new Exercise
        {
            TrainerId = trainer.Id,
            Name = "Agachamento",
            MuscleGroup = MuscleGroup.Legs,
            Equipment = Equipment.Barbell
        };
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();

        var req = new CreateWorkoutPlanRequest(
            student.Id, "Plano de Força", "Foco em pernas", null, null,
            [
                new CreateWorkoutDayRequest(DayOfWeek.Wednesday, "Treino A", null, 0,
                    [
                        new CreatePlanExerciseRequest(exercise.Id, 4, "8-10", 60, 90, 0, null)
                    ])
            ]);

        //Act
        var resultado = await _sut.CreatePlanAsync(trainer.Id, req);

        //Assert
        resultado.Should().NotBeNull();
        resultado.Name.Should().Be("Plano de Força");
        resultado.Days.Should().ContainSingle();
        resultado.Days[0].Exercises.Should().ContainSingle();
        resultado.Days[0].Exercises[0].Sets.Should().Be(4);
    }

    [Fact]
    public async Task CreatePlanAsync_StudentNaoPertenceAoTrainer_LancaExcecao()
    {
        //Arrange
        var (_, student) = await SeedTrainerComAlunoAsync();
        var req = new CreateWorkoutPlanRequest(student.Id, "Plano", null, null, null, []);

        //Act
        var act = () => _sut.CreatePlanAsync(Guid.NewGuid(), req);

        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── GetStudentDashboardAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetStudentDashboardAsync_SemPlanoAtivo_TodayENextSaoNulos()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday, isActive: false);

        //Act
        var resultado = await _sut.GetStudentDashboardAsync(student.Id);

        //Assert
        resultado.TodayWorkout.Should().BeNull();
        resultado.NextWorkout.Should().BeNull();
    }

    [Fact]
    public async Task GetStudentDashboardAsync_PlanoAtivoComTreinoHoje_RetornaTodayWorkout()
    {
        // O serviço lê DateTime.Today.DayOfWeek internamente (sem clock injetável),
        // então o dia usado no teste precisa ser calculado em cima do dia real de execução.
        var hoje = DateTime.Today.DayOfWeek;
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(trainer.Id, student.Id, hoje);

        //Act
        var resultado = await _sut.GetStudentDashboardAsync(student.Id);

        //Assert
        resultado.TodayWorkout.Should().NotBeNull();
        resultado.TodayWorkout!.DayOfWeek.Should().Be(hoje);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_SemTreinoHoje_RetornaProximoTreino()
    {
        var hoje = DateTime.Today.DayOfWeek;
        var amanha = (DayOfWeek)(((int)hoje + 1) % 7);
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        await SeedPlanComExercicioAsync(trainer.Id, student.Id, amanha);

        //Act
        var resultado = await _sut.GetStudentDashboardAsync(student.Id);

        //Assert
        resultado.TodayWorkout.Should().BeNull();
        resultado.NextWorkout.Should().NotBeNull();
        resultado.NextWorkout!.DayOfWeek.Should().Be(amanha);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_ContaSessoesDoMesAtual()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        _db.WorkoutSessions.AddRange(
            new WorkoutSession { StudentId = student.Id, WorkoutDayId = day.Id, StartedAt = DateTime.UtcNow },
            new WorkoutSession { StudentId = student.Id, WorkoutDayId = day.Id, StartedAt = DateTime.UtcNow.AddMonths(-2) }
        );
        await _db.SaveChangesAsync();

        //Act
        var resultado = await _sut.GetStudentDashboardAsync(student.Id);

        //Assert
        resultado.TotalSessionsThisMonth.Should().Be(1);
        resultado.TotalSessionsAllTime.Should().Be(2);
    }

    [Fact]
    public async Task GetStudentDashboardAsync_CalculaPersonalRecordPorMaiorPeso()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, planExercise) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        var session = new WorkoutSession { StudentId = student.Id, WorkoutDayId = day.Id, StartedAt = DateTime.UtcNow };
        _db.WorkoutSessions.Add(session);
        await _db.SaveChangesAsync();

        _db.SessionSets.AddRange(
            new SessionSet { SessionId = session.Id, PlanExerciseId = planExercise.Id, SetNumber = 1, RepsDone = 10, WeightKg = 40, LoggedAt = DateTime.UtcNow },
            new SessionSet { SessionId = session.Id, PlanExerciseId = planExercise.Id, SetNumber = 2, RepsDone = 8, WeightKg = 45, LoggedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        //Act
        var resultado = await _sut.GetStudentDashboardAsync(student.Id);

        //Assert
        resultado.RecentPRs.Should().ContainSingle();
        resultado.RecentPRs[0].WeightKg.Should().Be(45);
        resultado.RecentPRs[0].Reps.Should().Be(8);
    }

    // ─── StartSessionAsync / LogSetAsync / FinishSessionAsync / GetSessionAsync ─

    [Fact]
    public async Task StartSessionAsync_CriaSessaoComStartedAtPreenchido()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);

        //Act
        var resultado = await _sut.StartSessionAsync(student.Id, new StartSessionRequest(day.Id));

        //Assert
        resultado.Should().NotBeNull();
        resultado.WorkoutDayId.Should().Be(day.Id);
        resultado.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogSetAsync_SessaoDeOutroAluno_RetornaNull()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, planExercise) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);
        var session = await _sut.StartSessionAsync(student.Id, new StartSessionRequest(day.Id));

        //Act
        var resultado = await _sut.LogSetAsync(session.Id, Guid.NewGuid(), new LogSetRequest(planExercise.Id, 1, 10, 40));

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LogSetAsync_SessaoValida_AdicionaSet()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, planExercise) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);
        var session = await _sut.StartSessionAsync(student.Id, new StartSessionRequest(day.Id));

        //Act
        var resultado = await _sut.LogSetAsync(session.Id, student.Id, new LogSetRequest(planExercise.Id, 1, 10, 42.5f));

        //Assert
        resultado.Should().NotBeNull();
        resultado!.Sets.Should().ContainSingle();
        resultado.Sets[0].WeightKg.Should().Be(42.5f);
    }

    [Fact]
    public async Task FinishSessionAsync_CalculaDuracaoEmSegundos()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);
        var session = await _sut.StartSessionAsync(student.Id, new StartSessionRequest(day.Id));

        //Act
        var resultado = await _sut.FinishSessionAsync(session.Id, student.Id, new FinishSessionRequest(140, 300, "Bom treino"));

        //Assert
        resultado.Should().NotBeNull();
        resultado!.FinishedAt.Should().NotBeNull();
        resultado.DurationSeconds.Should().BeGreaterThanOrEqualTo(0);
        resultado.AvgHeartRate.Should().Be(140);
        resultado.CaloriesBurned.Should().Be(300);
    }

    [Fact]
    public async Task FinishSessionAsync_SessaoDeOutroAluno_RetornaNull()
    {
        //Arrange
        var (trainer, student) = await SeedTrainerComAlunoAsync();
        var (_, day, _) = await SeedPlanComExercicioAsync(trainer.Id, student.Id, DayOfWeek.Monday);
        var session = await _sut.StartSessionAsync(student.Id, new StartSessionRequest(day.Id));

        //Act
        var resultado = await _sut.FinishSessionAsync(session.Id, Guid.NewGuid(), new FinishSessionRequest(null, null, null));

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetSessionAsync_Inexistente_RetornaNull()
    {
        //Arrange
        var (_, student) = await SeedTrainerComAlunoAsync();

        //Act
        var resultado = await _sut.GetSessionAsync(Guid.NewGuid(), student.Id);

        //Assert
        resultado.Should().BeNull();
    }
}
