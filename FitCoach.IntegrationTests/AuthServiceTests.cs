using FitCoach.API.Data;
using FitCoach.API.Models;
using FitCoach.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitCoach.IntegrationTests;

public class AuthServiceTests : IDisposable
{
    private readonly FitCoachDbContext _db;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<FitCoachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _db = new FitCoachDbContext(options);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                ["Jwt:Key"] = "chave-super-secreta-para-testes-32chars!",
                ["Jwt:Issuer"] = "fitcoach-test",
                ["Jwt:Audience"] = "fitcoach-test",
            }!)
            .Build();
        
        _sut = new AuthService(_db, config);
    }

    /// <summary>
    /// Nomenclatura Padrão: Método_Cenário_ResultadoEsperado
    /// </summary>
    public async Task LoginAsync_CredenciaisCorretas_RetornaToken()
    {
        //Arrange
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("password");
        _db.Users.Add(new User()
        {
            Name = ""
        });
        //Act

        //Assert
    }
    
    public void Dispose() => _db.Dispose();
}