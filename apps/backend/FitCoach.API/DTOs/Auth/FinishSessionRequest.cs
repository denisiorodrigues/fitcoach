namespace FitCoach.API.DTOs.Auth;

public record FinishSessionRequest(
    int? AvgHeartRate,
    int? CaloriesBurned,
    string? StudentNotes
);
