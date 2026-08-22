using FitCoach.API.Data;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Trainer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitCoach.API.Controllers;

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
