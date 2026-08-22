using FitCoach.API.DTOs.Student;

namespace FitCoach.API.DTOs.Trainer;

public record TrainerDashboardDto(
    int TotalStudents,
    int ActiveStudentsThisWeek,
    int TotalPlans,
    List<StudentActivityDto> StudentActivity
);
