using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;

public class WorkoutDay
{
    [Key] 
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    [Required, MaxLength(60)] 
    public string Label { get; set; } // "Treino A"
    [MaxLength(200)] public string? Notes { get; set; }
    public int OrderIndex { get; set; }

    [ForeignKey("PlanId")] public WorkoutPlan Plan { get; set; }
    public ICollection<PlanExercise> Exercises { get; set; }
    public ICollection<WorkoutSession> Sessions { get; set; }
}
