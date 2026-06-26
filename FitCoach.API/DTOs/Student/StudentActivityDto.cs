namespace FitCoach.API.DTOs.Student;

public record StudentActivityDto(
    Guid StudentId,
    string StudentName,
    string? AvatarUrl,
    DateTime? LastSessionAt,
    int SessionsThisMonth,
    bool IsActive
);
