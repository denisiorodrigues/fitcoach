using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

// ─── Enums ────────────────────────────────────────────────────────────────────

public enum UserRole { Trainer, Student }
public enum MuscleGroup
{
    Chest, Back, Shoulders, Biceps, Triceps,
    Legs, Glutes, Core, FullBody, Cardio
}
public enum Equipment
{
    Barbell, Dumbbell, Cable, Machine,
    Bodyweight, ResistanceBand, Kettlebell, Other
}

// ─── User (shared for trainers and students) ──────────────────────────────────

public class User
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [Required, MaxLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public TrainerProfile? TrainerProfile { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}

// ─── Trainer Profile ──────────────────────────────────────────────────────────

public class TrainerProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [MaxLength(500)] public string? Bio { get; set; }
    [MaxLength(200)] public string? Specialty { get; set; }
    public string? CrefNumber { get; set; }

    // Navigation
    [ForeignKey("UserId")] public User User { get; set; } = null!;
    public ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
}

// ─── Student Profile ──────────────────────────────────────────────────────────

public class StudentProfile
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid TrainerId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public float? WeightKg { get; set; }
    public float? HeightCm { get; set; }
    [MaxLength(300)] public string? Goal { get; set; }
    [MaxLength(500)] public string? HealthNotes { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("UserId")] public User User { get; set; } = null!;
    [ForeignKey("TrainerId")] public TrainerProfile Trainer { get; set; } = null!;
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public ICollection<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
}

// ─── Exercise Library ─────────────────────────────────────────────────────────

public class Exercise
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainerId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
    [MaxLength(1000)] public string? Instructions { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsGlobal { get; set; } = false; // default library vs trainer-custom
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("TrainerId")] public TrainerProfile Trainer { get; set; } = null!;
    public ICollection<PlanExercise> PlanExercises { get; set; } = new List<PlanExercise>();
}

// ─── Workout Plan (prescribed by trainer for a student) ───────────────────────

public class WorkoutPlan
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("TrainerId")] public TrainerProfile Trainer { get; set; } = null!;
    [ForeignKey("StudentId")] public StudentProfile Student { get; set; } = null!;
    public ICollection<WorkoutDay> Days { get; set; } = new List<WorkoutDay>();
}

// ─── Workout Day (e.g. "Monday - Treino A - Peito e Tríceps") ────────────────

public class WorkoutDay
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlanId { get; set; }
    public int DayOfWeek { get; set; } // 0=Mon..6=Sun
    [Required, MaxLength(60)] public string Label { get; set; } = ""; // "Treino A"
    [MaxLength(200)] public string? Notes { get; set; }
    public int OrderIndex { get; set; }

    // Navigation
    [ForeignKey("PlanId")] public WorkoutPlan Plan { get; set; } = null!;
    public ICollection<PlanExercise> Exercises { get; set; } = new List<PlanExercise>();
    public ICollection<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
}

// ─── Plan Exercise (exercise within a workout day) ────────────────────────────

public class PlanExercise
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutDayId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; } = 3;
    [MaxLength(20)] public string Reps { get; set; } = "12"; // "12" or "8-12"
    public float? WeightKg { get; set; }
    public int RestSeconds { get; set; } = 90;
    public int OrderIndex { get; set; }
    [MaxLength(300)] public string? CoachNotes { get; set; }

    // Navigation
    [ForeignKey("WorkoutDayId")] public WorkoutDay WorkoutDay { get; set; } = null!;
    [ForeignKey("ExerciseId")] public Exercise Exercise { get; set; } = null!;
    public ICollection<SessionSet> SessionSets { get; set; } = new List<SessionSet>();
}

// ─── Workout Session (actual execution by the student) ────────────────────────

public class WorkoutSession
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid WorkoutDayId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public int? AvgHeartRate { get; set; }
    public int? CaloriesBurned { get; set; }
    public int? DurationSeconds { get; set; }
    [MaxLength(500)] public string? StudentNotes { get; set; }

    // Navigation
    [ForeignKey("StudentId")] public StudentProfile Student { get; set; } = null!;
    [ForeignKey("WorkoutDayId")] public WorkoutDay WorkoutDay { get; set; } = null!;
    public ICollection<SessionSet> Sets { get; set; } = new List<SessionSet>();
}

// ─── Session Set (individual set logged during session) ───────────────────────

public class SessionSet
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid PlanExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int RepsDone { get; set; }
    public float WeightKg { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("SessionId")] public WorkoutSession Session { get; set; } = null!;
    [ForeignKey("PlanExerciseId")] public PlanExercise PlanExercise { get; set; } = null!;
}
