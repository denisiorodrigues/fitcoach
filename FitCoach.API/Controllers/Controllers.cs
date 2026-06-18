using System.Security.Claims;
using FitCoach.API.Data;
using FitCoach.API.DTOs;
using FitCoach.API.Models;
using FitCoach.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitCoach.API.Controllers;

// ─── Auth ─────────────────────────────────────────────────────────────────────

[ApiController, Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var result = await authService.LoginAsync(req);
        return result is null ? Unauthorized("Credenciais inválidas.") : Ok(result);
    }

    [HttpPost("register/trainer")]
    public async Task<IActionResult> RegisterTrainer(RegisterTrainerRequest req)
    {
        try
        {
            var result = await authService.RegisterTrainerAsync(req);
            return CreatedAtAction(null, result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPost("register/student"), Authorize(Roles = "Trainer")]
    public async Task<IActionResult> RegisterStudent(RegisterStudentRequest req)
    {
        var trainerId = GetProfileId();
        try
        {
            var result = await authService.RegisterStudentAsync(req, trainerId);
            return CreatedAtAction(null, result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Students (Trainer manages) ───────────────────────────────────────────────

[ApiController, Route("api/students"), Authorize(Roles = "Trainer")]
public class StudentsController(FitCoachDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyStudents()
    {
        var trainerId = GetProfileId();
        var students = await db.StudentProfiles
            .Where(s => s.TrainerId == trainerId)
            .Include(s => s.User)
            .Include(s => s.Sessions)
            .Select(s => new StudentProfileDto(
                s.Id, new UserDto(s.User.Id, s.User.Name, s.User.Email, "Student", s.User.AvatarUrl),
                s.TrainerId, s.Trainer.User.Name,
                s.BirthDate, s.WeightKg, s.HeightCm, s.Goal,
                s.EnrolledAt, s.Sessions.Count
            ))
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var trainerId = GetProfileId();
        var student = await db.StudentProfiles
            .Where(s => s.Id == id && s.TrainerId == trainerId)
            .Include(s => s.User)
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.Sessions)
            .FirstOrDefaultAsync();

        if (student is null) return NotFound();

        return Ok(new StudentProfileDto(
            student.Id,
            new UserDto(student.User.Id, student.User.Name, student.User.Email, "Student", student.User.AvatarUrl),
            student.TrainerId, student.Trainer.User.Name,
            student.BirthDate, student.WeightKg, student.HeightCm, student.Goal,
            student.EnrolledAt, student.Sessions.Count
        ));
    }

    [HttpGet("{id:guid}/activity")]
    public async Task<IActionResult> GetStudentActivity(Guid id)
    {
        var trainerId = GetProfileId();
        var student = await db.StudentProfiles
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (student is null) return NotFound();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var recentSessions = await db.WorkoutSessions
            .Where(s => s.StudentId == id)
            .Include(s => s.WorkoutDay)
            .Include(s => s.Sets).ThenInclude(ss => ss.PlanExercise).ThenInclude(pe => pe.Exercise)
            .OrderByDescending(s => s.StartedAt)
            .Take(10)
            .ToListAsync();

        return Ok(new
        {
            SessionsThisMonth = recentSessions.Count(s => s.StartedAt >= monthStart),
            TotalSessions = await db.WorkoutSessions.CountAsync(s => s.StudentId == id),
            LastSession = recentSessions.FirstOrDefault()?.StartedAt,
            RecentSessions = recentSessions
        });
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Exercises ────────────────────────────────────────────────────────────────

[ApiController, Route("api/exercises")]
[Authorize]
public class ExercisesController(FitCoachDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetExercises([FromQuery] string? muscle, [FromQuery] string? equipment)
    {
        var trainerId = GetProfileId();
        var isTrainer = User.IsInRole("Trainer");

        var query = db.Exercises
            .Where(e => e.IsGlobal || (isTrainer && e.TrainerId == trainerId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(muscle) && Enum.TryParse<MuscleGroup>(muscle, out var mg))
            query = query.Where(e => e.MuscleGroup == mg);

        if (!string.IsNullOrEmpty(equipment) && Enum.TryParse<Equipment>(equipment, out var eq))
            query = query.Where(e => e.Equipment == eq);

        var exercises = await query
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseDto(e.Id, e.Name, e.MuscleGroup.ToString(), e.Equipment.ToString(),
                e.Instructions, e.VideoUrl, e.ThumbnailUrl, e.IsGlobal))
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpPost, Authorize(Roles = "Trainer")]
    public async Task<IActionResult> CreateExercise(CreateExerciseRequest req)
    {
        var trainerId = GetProfileId();

        if (!Enum.TryParse<MuscleGroup>(req.MuscleGroup, out var mg))
            return BadRequest("Grupo muscular inválido.");
        if (!Enum.TryParse<Equipment>(req.Equipment, out var eq))
            return BadRequest("Equipamento inválido.");

        var exercise = new Exercise
        {
            TrainerId = trainerId,
            Name = req.Name,
            MuscleGroup = mg,
            Equipment = eq,
            Instructions = req.Instructions,
            VideoUrl = req.VideoUrl,
            ThumbnailUrl = req.ThumbnailUrl
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExercises), new { id = exercise.Id },
            new ExerciseDto(exercise.Id, exercise.Name, mg.ToString(), eq.ToString(),
                exercise.Instructions, exercise.VideoUrl, exercise.ThumbnailUrl, false));
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Workout Plans ────────────────────────────────────────────────────────────

[ApiController, Route("api/plans")]
[Authorize]
public class WorkoutPlansController(WorkoutService workoutService) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Trainer")]
    public async Task<IActionResult> GetMyPlans()
    {
        var trainerId = GetProfileId();
        return Ok(await workoutService.GetTrainerPlansAsync(trainerId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlan(Guid id)
    {
        var profileId = GetProfileId();
        var isTrainer = User.IsInRole("Trainer");
        var plan = await workoutService.GetPlanDetailAsync(id, profileId, isTrainer);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost, Authorize(Roles = "Trainer")]
    public async Task<IActionResult> CreatePlan(CreateWorkoutPlanRequest req)
    {
        var trainerId = GetProfileId();
        try
        {
            var plan = await workoutService.CreatePlanAsync(trainerId, req);
            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Student Dashboard ────────────────────────────────────────────────────────

[ApiController, Route("api/dashboard")]
[Authorize(Roles = "Student")]
public class DashboardController(WorkoutService workoutService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var studentId = GetProfileId();
        var dashboard = await workoutService.GetStudentDashboardAsync(studentId);
        return Ok(dashboard);
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Workout Sessions ─────────────────────────────────────────────────────────

[ApiController, Route("api/sessions")]
[Authorize(Roles = "Student")]
public class SessionsController(WorkoutService workoutService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> StartSession(StartSessionRequest req)
    {
        var studentId = GetProfileId();
        var session = await workoutService.StartSessionAsync(studentId, req);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var studentId = GetProfileId();
        var session = await workoutService.GetSessionAsync(id, studentId);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost("{id:guid}/sets")]
    public async Task<IActionResult> LogSet(Guid id, LogSetRequest req)
    {
        var studentId = GetProfileId();
        var session = await workoutService.LogSetAsync(id, studentId, req);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<IActionResult> FinishSession(Guid id, FinishSessionRequest req)
    {
        var studentId = GetProfileId();
        var session = await workoutService.FinishSessionAsync(id, studentId, req);
        return session is null ? NotFound() : Ok(session);
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}

// ─── Trainer Dashboard ────────────────────────────────────────────────────────

[ApiController, Route("api/trainer/dashboard")]
[Authorize(Roles = "Trainer")]
public class TrainerDashboardController(FitCoachDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var trainerId = GetProfileId();
        var now = DateTime.UtcNow;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);

        var students = await db.StudentProfiles
            .Where(s => s.TrainerId == trainerId)
            .Include(s => s.User)
            .Include(s => s.Sessions)
            .ToListAsync();

        var totalPlans = await db.WorkoutPlans.CountAsync(p => p.TrainerId == trainerId);

        var activity = students.Select(s =>
        {
            var lastSession = s.Sessions.MaxBy(ss => ss.StartedAt);
            var sessionsThisMonth = s.Sessions.Count(ss =>
                ss.StartedAt >= new DateTime(now.Year, now.Month, 1));

            return new StudentActivityDto(
                s.Id, s.User.Name, s.User.AvatarUrl,
                lastSession?.StartedAt,
                sessionsThisMonth,
                lastSession?.StartedAt > weekStart
            );
        }).OrderByDescending(a => a.LastSessionAt).ToList();

        var activeThisWeek = activity.Count(a => a.IsActive);

        return Ok(new TrainerDashboardDto(
            students.Count, activeThisWeek, totalPlans, activity
        ));
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}
