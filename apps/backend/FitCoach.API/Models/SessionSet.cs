using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitCoach.API.Models;
public class SessionSet
{
    [Key] 
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid PlanExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int RepsDone { get; set; }
    public float WeightKg { get; set; }
    public DateTime LoggedAt { get; set; }

    [ForeignKey("SessionId")] 
    public WorkoutSession Session { get; set; } = null!;
    
    [ForeignKey("PlanExerciseId")] 
    public PlanExercise PlanExercise { get; set; } = null!;
}
