using FitCoach.API.Data;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Workout;
using FitCoach.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FitCoach.API.Services;

public class WorkoutService(FitCoachDbContext db)
{
    // ─── Plans ────────────────────────────────────────────────────────────────

    public async Task<List<WorkoutPlanSummaryDto>> GetTrainerPlansAsync(Guid trainerId)
    {
        return await db.WorkoutPlans
            .Where(p => p.TrainerId == trainerId)
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Days)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new WorkoutPlanSummaryDto(
                p.Id, p.Name, p.Description,
                p.StudentId, p.Student.User.Name,
                p.IsActive, p.Days.Count, p.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<WorkoutPlanDto?> GetPlanDetailAsync(Guid planId, Guid requesterId, bool isTrainer)
    {
        var plan = await db.WorkoutPlans
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.Days.OrderBy(d => d.OrderIndex))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan is null) return null;

        // Authorization: trainer owns it OR student is the subject
        if (isTrainer && plan.TrainerId != requesterId) return null;
        if (!isTrainer && plan.StudentId != requesterId) return null;

        return MapPlanToDto(plan);
    }

    public async Task<WorkoutPlanDto> CreatePlanAsync(Guid trainerId, CreateWorkoutPlanRequest req)
    {
        var student = await db.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == req.StudentId && s.TrainerId == trainerId)
            ?? throw new InvalidOperationException("Aluno não encontrado para este trainer.");

        var plan = new WorkoutPlan
        {
            TrainerId = trainerId,
            StudentId = req.StudentId,
            Name = req.Name,
            Description = req.Description,
            StartDate = req.StartDate,
            EndDate = req.EndDate
        };

        foreach (var (dayReq, i) in req.Days.Select((d, i) => (d, i)))
        {
            var day = new WorkoutDay
            {
                DayOfWeek = dayReq.DayOfWeek,
                Label = dayReq.Label,
                Notes = dayReq.Notes,
                OrderIndex = dayReq.OrderIndex
            };

            foreach (var (exReq, j) in dayReq.Exercises.Select((e, j) => (e, j)))
            {
                day.Exercises.Add(new PlanExercise
                {
                    ExerciseId = exReq.ExerciseId,
                    Sets = exReq.Sets,
                    Reps = exReq.Reps,
                    WeightKg = exReq.WeightKg,
                    RestSeconds = exReq.RestSeconds,
                    OrderIndex = exReq.OrderIndex,
                    CoachNotes = exReq.CoachNotes
                });
            }
            plan.Days.Add(day);
        }

        db.WorkoutPlans.Add(plan);
        await db.SaveChangesAsync();

        return (await GetPlanDetailAsync(plan.Id, trainerId, true))!;
    }

    // ─── Student Dashboard ────────────────────────────────────────────────────

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentProfileId)
    {
        var today = DateTime.Today.DayOfWeek; // 0=Sun in .NET, remap to 0=Mon

        var activePlan = await db.WorkoutPlans
            .Include(p => p.Days.OrderBy(d => d.OrderIndex))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(pe => pe.Exercise)
            .Where(p => p.StudentId == studentProfileId && p.IsActive)
            .FirstOrDefaultAsync();

        WorkoutDayDto? todayWorkout = null;
        WorkoutDayDto? nextWorkout = null;

        if (activePlan is not null)
        {
            var todayDay = activePlan.Days.FirstOrDefault(d => d.DayOfWeek == today);
            todayWorkout = todayDay is not null ? MapDayToDto(todayDay) : null;

            // Next upcoming day
            var nextDay = activePlan.Days
                .Where(d => d.DayOfWeek != today)
                .OrderBy(d => (d.DayOfWeek - today + 7) % 7)
                .FirstOrDefault();
            nextWorkout = nextDay is not null ? MapDayToDto(nextDay) : null;
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var sessionsThisMonth = await db.WorkoutSessions
            .CountAsync(s => s.StudentId == studentProfileId && s.StartedAt >= monthStart);

        var totalSessions = await db.WorkoutSessions
            .CountAsync(s => s.StudentId == studentProfileId);

        var recentSessions = await db.WorkoutSessions
            .Where(s => s.StudentId == studentProfileId)
            .Include(s => s.WorkoutDay)
            .Include(s => s.Sets).ThenInclude(ss => ss.PlanExercise).ThenInclude(pe => pe.Exercise)
            .OrderByDescending(s => s.StartedAt)
            .Take(5)
            .Select(s => MapSessionToDto(s))
            .ToListAsync();

        // Personal records: max weight per exercise
        var prs = await db.SessionSets
            .Where(ss => ss.Session.StudentId == studentProfileId)
            .GroupBy(ss => ss.PlanExercise.Exercise.Name)
            .Select(g => new PersonalRecordDto(
                g.Key,
                g.Max(ss => ss.WeightKg),
                g.OrderByDescending(ss => ss.WeightKg).First().RepsDone,
                g.OrderByDescending(ss => ss.WeightKg).First().LoggedAt
            ))
            .OrderByDescending(pr => pr.AchievedAt)
            .Take(5)
            .ToListAsync();

        return new StudentDashboardDto(
            todayWorkout, nextWorkout,
            sessionsThisMonth, totalSessions,
            prs, recentSessions
        );
    }

    // ─── Sessions ─────────────────────────────────────────────────────────────

    public async Task<WorkoutSessionDto> StartSessionAsync(Guid studentProfileId, StartSessionRequest req)
    {
        var session = new WorkoutSession
        {
            StudentId = studentProfileId,
            WorkoutDayId = req.WorkoutDayId,
            StartedAt = DateTime.UtcNow
        };
        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync();

        return await GetSessionAsync(session.Id, studentProfileId)
            ?? throw new Exception("Session not found after creation");
    }

    public async Task<WorkoutSessionDto?> LogSetAsync(Guid sessionId, Guid studentProfileId, LogSetRequest req)
    {
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.StudentId == studentProfileId);
        if (session is null) return null;

        db.SessionSets.Add(new SessionSet
        {
            SessionId = sessionId,
            PlanExerciseId = req.PlanExerciseId,
            SetNumber = req.SetNumber,
            RepsDone = req.RepsDone,
            WeightKg = req.WeightKg,
            LoggedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return await GetSessionAsync(sessionId, studentProfileId);
    }

    public async Task<WorkoutSessionDto?> FinishSessionAsync(Guid sessionId, Guid studentProfileId, FinishSessionRequest req)
    {
        var session = await db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.StudentId == studentProfileId);
        if (session is null) return null;

        session.FinishedAt = DateTime.UtcNow;
        session.DurationSeconds = (int)(session.FinishedAt.Value - session.StartedAt).TotalSeconds;
        session.AvgHeartRate = req.AvgHeartRate;
        session.CaloriesBurned = req.CaloriesBurned;
        session.StudentNotes = req.StudentNotes;
        await db.SaveChangesAsync();

        return await GetSessionAsync(sessionId, studentProfileId);
    }

    public async Task<WorkoutSessionDto?> GetSessionAsync(Guid sessionId, Guid studentProfileId)
    {
        var session = await db.WorkoutSessions
            .Where(s => s.Id == sessionId && s.StudentId == studentProfileId)
            .Include(s => s.WorkoutDay)
            .Include(s => s.Sets)
                .ThenInclude(ss => ss.PlanExercise)
                    .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync();

        return session is null ? null : MapSessionToDto(session);
    }

    // ─── Mappers ──────────────────────────────────────────────────────────────

    private static WorkoutPlanDto MapPlanToDto(WorkoutPlan p) => new(
        p.Id, p.Name, p.Description,
        p.StudentId, p.Student.User.Name,
        p.StartDate, p.EndDate, p.IsActive,
        p.Days.Select(MapDayToDto).ToList(),
        p.CreatedAt
    );

    private static WorkoutDayDto MapDayToDto(WorkoutDay d) => new(
        d.Id, d.DayOfWeek, d.Label, d.Notes, d.OrderIndex,
        d.Exercises.Select(MapPlanExerciseToDto).ToList()
    );

    private static PlanExerciseDto MapPlanExerciseToDto(PlanExercise pe) => new(
        pe.Id,
        new ExerciseDto(pe.Exercise.Id, pe.Exercise.Name, pe.Exercise.MuscleGroup.ToString(),
            pe.Exercise.Equipment.ToString(), pe.Exercise.Instructions,
            pe.Exercise.VideoUrl, pe.Exercise.ThumbnailUrl, pe.Exercise.IsGlobal),
        pe.Sets, pe.Reps, pe.WeightKg, pe.RestSeconds, pe.OrderIndex, pe.CoachNotes
    );

    private static WorkoutSessionDto MapSessionToDto(WorkoutSession s) => new(
        s.Id, s.WorkoutDayId, s.WorkoutDay.Label,
        s.StartedAt, s.FinishedAt, s.DurationSeconds,
        s.AvgHeartRate, s.CaloriesBurned, s.StudentNotes,
        s.Sets.Select(ss => new SessionSetDto(
            ss.Id, ss.PlanExerciseId, ss.PlanExercise.Exercise.Name,
            ss.SetNumber, ss.RepsDone, ss.WeightKg, ss.LoggedAt
        )).ToList()
    );
}
