using Microsoft.EntityFrameworkCore;
using FitCoach.API.Models;

namespace FitCoach.API.Data;

public class FitCoachDbContext : DbContext
{
    public FitCoachDbContext(DbContextOptions<FitCoachDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TrainerProfile> TrainerProfiles { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
    public DbSet<WorkoutDay> WorkoutDays { get; set; }
    public DbSet<PlanExercise> PlanExercises { get; set; }
    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<SessionSet> SessionSets { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── User ──────────────────────────────────────────────────────────────
        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        // ── Trainer ───────────────────────────────────────────────────────────
        mb.Entity<TrainerProfile>(e =>
        {
            e.HasOne(t => t.User)
             .WithOne(u => u.TrainerProfile)
             .HasForeignKey<TrainerProfile>(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Student ───────────────────────────────────────────────────────────
        mb.Entity<StudentProfile>(e =>
        {
            e.HasOne(s => s.User)
             .WithOne(u => u.StudentProfile)
             .HasForeignKey<StudentProfile>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Trainer)
             .WithMany(t => t.Students)
             .HasForeignKey(s => s.TrainerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Exercise ──────────────────────────────────────────────────────────
        mb.Entity<Exercise>(e =>
        {
            e.Property(x => x.MuscleGroup).HasConversion<string>();
            e.Property(x => x.Equipment).HasConversion<string>();
            e.HasOne(x => x.Trainer)
             .WithMany(t => t.Exercises)
             .HasForeignKey(x => x.TrainerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WorkoutPlan ───────────────────────────────────────────────────────
        mb.Entity<WorkoutPlan>(e =>
        {
            e.HasOne(p => p.Trainer)
             .WithMany(t => t.WorkoutPlans)
             .HasForeignKey(p => p.TrainerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Student)
             .WithMany(s => s.WorkoutPlans)
             .HasForeignKey(p => p.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── WorkoutDay ────────────────────────────────────────────────────────
        mb.Entity<WorkoutDay>(e =>
        {
            e.HasOne(d => d.Plan)
             .WithMany(p => p.Days)
             .HasForeignKey(d => d.PlanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PlanExercise ──────────────────────────────────────────────────────
        mb.Entity<PlanExercise>(e =>
        {
            e.HasOne(pe => pe.WorkoutDay)
             .WithMany(d => d.Exercises)
             .HasForeignKey(pe => pe.WorkoutDayId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pe => pe.Exercise)
             .WithMany(x => x.PlanExercises)
             .HasForeignKey(pe => pe.ExerciseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WorkoutSession ────────────────────────────────────────────────────
        mb.Entity<WorkoutSession>(e =>
        {
            e.HasOne(s => s.Student)
             .WithMany(st => st.Sessions)
             .HasForeignKey(s => s.StudentId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.WorkoutDay)
             .WithMany(d => d.Sessions)
             .HasForeignKey(s => s.WorkoutDayId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SessionSet ────────────────────────────────────────────────────────
        mb.Entity<SessionSet>(e =>
        {
            e.HasOne(ss => ss.Session)
             .WithMany(s => s.Sets)
             .HasForeignKey(ss => ss.SessionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ss => ss.PlanExercise)
             .WithMany(pe => pe.SessionSets)
             .HasForeignKey(ss => ss.PlanExerciseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed: default exercise library ────────────────────────────────────
        SeedDefaultExercises(mb);
    }

    private static void SeedDefaultExercises(ModelBuilder mb)
    {
        // System trainer for global exercises
        var systemTrainerId = new Guid("00000000-0000-0000-0000-000000000001");

        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Supino Reto", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Supino Inclinado", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Dumbbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Crucifixo", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Dumbbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Agachamento Livre", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Leg Press", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Machine, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Levantamento Terra", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Puxada Frontal", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Cable, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Remada Curvada", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Barra Fixa", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Desenvolvimento", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Elevação Lateral", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Dumbbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Rosca Direta", MuscleGroup = MuscleGroup.Biceps, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Rosca Martelo", MuscleGroup = MuscleGroup.Biceps, Equipment = Equipment.Dumbbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Tríceps Testa", MuscleGroup = MuscleGroup.Triceps, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Tríceps Pulley", MuscleGroup = MuscleGroup.Triceps, Equipment = Equipment.Cable, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Hip Thrust", MuscleGroup = MuscleGroup.Glutes, Equipment = Equipment.Barbell, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Prancha", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Bodyweight, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Abdominal Supra", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Bodyweight, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Corrida na Esteira", MuscleGroup = MuscleGroup.Cardio, Equipment = Equipment.Machine, IsGlobal = true },
            new Exercise { Id = Guid.NewGuid(), TrainerId = systemTrainerId, Name = "Bicicleta Ergométrica", MuscleGroup = MuscleGroup.Cardio, Equipment = Equipment.Machine, IsGlobal = true },
        };
    }
}
