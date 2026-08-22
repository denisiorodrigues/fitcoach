using System.Security.Claims;
using FitCoach.API.Data;
using FitCoach.API.DTOs.Exercise;
using FitCoach.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitCoach.API.Controllers;

[ApiController, Route("api/exercises"), Authorize]
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
