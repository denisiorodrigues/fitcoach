namespace FitCoach.API.DTOs.Workout;

public record WorkoutPlanDto(
    Guid Id,
    string Name,
    string? Description,
    Guid StudentId,
    string StudentName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive,
    List<WorkoutDayDto> Days,
    DateTime CreatedAt
);
