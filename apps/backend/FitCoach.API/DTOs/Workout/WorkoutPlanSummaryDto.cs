namespace FitCoach.API.DTOs.Workout;

public record WorkoutPlanSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid StudentId,
    string StudentName,
    bool IsActive,
    int TotalDays,
    DateTime CreatedAt
);
