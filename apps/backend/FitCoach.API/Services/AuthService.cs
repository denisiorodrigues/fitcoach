using FitCoach.API.Data;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Trainer;
using FitCoach.API.DTOs.User;
using FitCoach.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FitCoach.API.Services;

public class AuthService(FitCoachDbContext db, IConfiguration config)
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var user = await db.Users
            .Include(u => u.TrainerProfile)
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower() && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return null;

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterTrainerAsync(RegisterTrainerRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            throw new InvalidOperationException("Email já cadastrado.");

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.Trainer
        };
        db.Users.Add(user);

        var profile = new TrainerProfile
        {
            UserId = user.Id,
            Specialty = req.Specialty,
            CrefNumber = req.CrefNumber
        };
        db.TrainerProfiles.Add(profile);

        await db.SaveChangesAsync();

        user.TrainerProfile = profile;
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterStudentAsync(RegisterStudentRequest req, Guid trainerId)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            throw new InvalidOperationException("Email já cadastrado.");

        var trainer = await db.TrainerProfiles.FindAsync(trainerId)
            ?? throw new InvalidOperationException("Trainer não encontrado.");

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.Student
        };
        db.Users.Add(user);

        var profile = new StudentProfile
        {
            UserId = user.Id,
            TrainerId = trainerId
        };
        db.StudentProfiles.Add(profile);

        await db.SaveChangesAsync();

        user.StudentProfile = profile;
        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = GenerateJwtToken(user);
        var refresh = GenerateRefreshToken();

        return new AuthResponse(
            token,
            refresh,
            new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.AvatarUrl)
        );
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Include profile ID in claims for easy lookup
        var profileId = user.Role == UserRole.Trainer
            ? user.TrainerProfile?.Id.ToString()
            : user.StudentProfile?.Id.ToString();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("profileId", profileId ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
