namespace FitCoach.API.DTOs.Workout;

public record CreateWorkoutPlanRequest(
    Guid StudentId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    List<CreateWorkoutDayRequest> Days
);
