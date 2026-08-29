using FitCoach.API.DTOs.Auth;

namespace FitCoach.API.DTOs.Workout;

public record WorkoutSessionDto(
    Guid Id,
    Guid WorkoutDayId,
    string WorkoutDayLabel,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int? DurationSeconds,
    int? AvgHeartRate,
    int? CaloriesBurned,
    string? StudentNotes,
    List<SessionSetDto> Sets
);
