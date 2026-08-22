using FitCoach.API.DTOs.Auth;
using FitCoach.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitCoach.API.Controllers;

[ApiController, Route("api/sessions"), Authorize(Roles = "Student")]
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
