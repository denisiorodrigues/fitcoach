using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Workout;

namespace FitCoach.API.DTOs.Student;

public record StudentDashboardDto(
    WorkoutDayDto? TodayWorkout,
    WorkoutDayDto? NextWorkout,
    int TotalSessionsThisMonth,
    int TotalSessionsAllTime,
    List<PersonalRecordDto> RecentPRs,
    List<WorkoutSessionDto> RecentSessions
);
