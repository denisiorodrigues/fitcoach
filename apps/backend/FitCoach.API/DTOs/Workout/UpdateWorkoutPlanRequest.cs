namespace FitCoach.API.DTOs.Workout;

public record UpdateWorkoutPlanRequest(
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive
);
