using Bogus;
using FitCoach.API.Models;

namespace FitCoach.UnitTests.Fakes;

public static class UserFaker
{
    public static Faker<User> Default() => new Faker<User>()
        .RuleFor(u => u.Name, f => f.Name.FullName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("senha123"))
        .RuleFor(u => u.Role, UserRole.Trainer)
        .RuleFor(u => u.IsActive, true);

}