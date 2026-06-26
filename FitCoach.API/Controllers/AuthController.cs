using System.Security.Claims;
using FitCoach.API.Data;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Trainer;
using FitCoach.API.Models;
using FitCoach.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitCoach.API.Controllers;

// ─── Auth ─────────────────────────────────────────────────────────────────────

[ApiController, Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var result = await authService.LoginAsync(req);
        return result is null ? Unauthorized("Credenciais inválidas.") : Ok(result);
    }

    [HttpPost("register/trainer")]
    public async Task<IActionResult> RegisterTrainer(RegisterTrainerRequest req)
    {
        try
        {
            var result = await authService.RegisterTrainerAsync(req);
            return CreatedAtAction(null, result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPost("register/student"), Authorize(Roles = "Trainer")]
    public async Task<IActionResult> RegisterStudent(RegisterStudentRequest req)
    {
        var trainerId = GetProfileId();
        try
        {
            var result = await authService.RegisterStudentAsync(req, trainerId);
            return CreatedAtAction(null, result);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    private Guid GetProfileId() =>
        Guid.Parse(User.FindFirstValue("profileId")!);
}
