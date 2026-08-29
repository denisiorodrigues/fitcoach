using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class StudentProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TrainerId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public float? WeightKg { get; set; }
    public float? HeightCm { get; set; }
    [MaxLength(300)] 
    public string? Goal { get; set; }
    [MaxLength(500)] 
    public string? HealthNotes { get; set; }
    public DateTime EnrolledAt { get; set; }

    [ForeignKey("UserId")] 
    public User User { get; set; }

    [ForeignKey("TrainerId")] 
    public TrainerProfile Trainer { get; set; }
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; }
    public ICollection<WorkoutSession> Sessions { get; set; }
}
