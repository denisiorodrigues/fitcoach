using System.Security.Claims;
using FitCoach.API.DTOs.Workout;
using FitCoach.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCoach.API.Controllers;

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
