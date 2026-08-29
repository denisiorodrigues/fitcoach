namespace FitCoach.API.DTOs.Student;

public record UpdateStudentProfileRequest(
    DateOnly? BirthDate,
    float? WeightKg,
    float? HeightCm,
    string? Goal,
    string? HealthNotes
);
