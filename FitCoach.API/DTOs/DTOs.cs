namespace FitCoach.API.DTOs;

// ─── Auth ─────────────────────────────────────────────────────────────────────

public record RegisterTrainerRequest(
    string Name,
    string Email,
    string Password,
    string? Specialty,
    string? CrefNumber
);

public record RegisterStudentRequest(
    string Name,
    string Email,
    string Password,
    string TrainerInviteCode   // trainer shares this code to onboard students
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User
);

// ─── User / Profile ───────────────────────────────────────────────────────────

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? AvatarUrl
);

public record TrainerProfileDto(
    Guid Id,
    UserDto User,
    string? Bio,
    string? Specialty,
    string? CrefNumber,
    int TotalStudents,
    int TotalPlans
);

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

public record UpdateStudentProfileRequest(
    DateOnly? BirthDate,
    float? WeightKg,
    float? HeightCm,
    string? Goal,
    string? HealthNotes
);

// ─── Exercise ─────────────────────────────────────────────────────────────────

public record ExerciseDto(
    Guid Id,
    string Name,
    string MuscleGroup,
    string Equipment,
    string? Instructions,
    string? VideoUrl,
    string? ThumbnailUrl,
    bool IsGlobal
);

public record CreateExerciseRequest(
    string Name,
    string MuscleGroup,
    string Equipment,
    string? Instructions,
    string? VideoUrl,
    string? ThumbnailUrl
);

// ─── Workout Plan ─────────────────────────────────────────────────────────────

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

public record CreateWorkoutPlanRequest(
    Guid StudentId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    List<CreateWorkoutDayRequest> Days
);

public record UpdateWorkoutPlanRequest(
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive
);

// ─── Workout Day ─────────────────────────────────────────────────────────────

public record WorkoutDayDto(
    Guid Id,
    int DayOfWeek,
    string Label,
    string? Notes,
    int OrderIndex,
    List<PlanExerciseDto> Exercises
);

public record CreateWorkoutDayRequest(
    int DayOfWeek,
    string Label,
    string? Notes,
    int OrderIndex,
    List<CreatePlanExerciseRequest> Exercises
);

// ─── Plan Exercise ────────────────────────────────────────────────────────────

public record PlanExerciseDto(
    Guid Id,
    ExerciseDto Exercise,
    int Sets,
    string Reps,
    float? WeightKg,
    int RestSeconds,
    int OrderIndex,
    string? CoachNotes
);

public record CreatePlanExerciseRequest(
    Guid ExerciseId,
    int Sets,
    string Reps,
    float? WeightKg,
    int RestSeconds,
    int OrderIndex,
    string? CoachNotes
);

// ─── Workout Session ──────────────────────────────────────────────────────────

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

public record StartSessionRequest(Guid WorkoutDayId);

public record FinishSessionRequest(
    int? AvgHeartRate,
    int? CaloriesBurned,
    string? StudentNotes
);

public record LogSetRequest(
    Guid PlanExerciseId,
    int SetNumber,
    int RepsDone,
    float WeightKg
);

public record SessionSetDto(
    Guid Id,
    Guid PlanExerciseId,
    string ExerciseName,
    int SetNumber,
    int RepsDone,
    float WeightKg,
    DateTime LoggedAt
);

// ─── Student Progress / Dashboard ────────────────────────────────────────────

public record StudentDashboardDto(
    WorkoutDayDto? TodayWorkout,
    WorkoutDayDto? NextWorkout,
    int TotalSessionsThisMonth,
    int TotalSessionsAllTime,
    List<PersonalRecordDto> RecentPRs,
    List<WorkoutSessionDto> RecentSessions
);

public record PersonalRecordDto(
    string ExerciseName,
    float WeightKg,
    int Reps,
    DateTime AchievedAt
);

// ─── Trainer Dashboard ────────────────────────────────────────────────────────

public record TrainerDashboardDto(
    int TotalStudents,
    int ActiveStudentsThisWeek,
    int TotalPlans,
    List<StudentActivityDto> StudentActivity
);

public record StudentActivityDto(
    Guid StudentId,
    string StudentName,
    string? AvatarUrl,
    DateTime? LastSessionAt,
    int SessionsThisMonth,
    bool IsActive
);
