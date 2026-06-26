using FitCoach.API.Data;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitCoach.API.Controllers;

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
