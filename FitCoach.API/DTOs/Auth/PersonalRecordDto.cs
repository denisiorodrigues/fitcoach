namespace FitCoach.API.DTOs.Auth;

public record PersonalRecordDto(
    string ExerciseName,
    float WeightKg,
    int Reps,
    DateTime AchievedAt
);
