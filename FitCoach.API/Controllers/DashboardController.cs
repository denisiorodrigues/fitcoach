using System.Security.Claims;
using FitCoach.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCoach.API.Controllers;

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
