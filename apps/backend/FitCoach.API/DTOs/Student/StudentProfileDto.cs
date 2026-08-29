using FitCoach.API.DTOs.User;

namespace FitCoach.API.DTOs.Student;

public record StudentProfileDto(
    Guid Id,
    UserDto User,
    Guid TrainerId,
    string TrainerName,
    DateOnly? BirthDate,
    float? WeightKg,
    float? HeightCm,
    string? Goal,
    DateTime EnrolledAt,
    int TotalSessions
);
