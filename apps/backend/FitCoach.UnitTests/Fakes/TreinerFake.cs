using Bogus;
using FitCoach.API.Models;

namespace FitCoach.UnitTests.Fakes;

public static class TreinerFake
{
    public static Faker<TrainerProfile> Default(Guid userId) => new Faker<TrainerProfile>()
        .RuleFor(t => t.UserId, userId)
        .RuleFor(t => t.Bio, f => f.Lorem.Paragraphs())
        .RuleFor(t => t.Specialty, f => f.Name.JobArea())
        .RuleFor(t => t.CrefNumber, f => f.Random.Replace("######-G/##"));
}