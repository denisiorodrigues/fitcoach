using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FitCoach.API.Data;
using FitCoach.API.DTOs.Auth;
using FitCoach.API.DTOs.Student;
using FitCoach.API.DTOs.Trainer;
using FitCoach.API.Models;
using FitCoach.API.Services;
using FitCoach.UnitTests.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitCoach.IntegrationTests;

public class AuthServiceTests : IDisposable
{
    private readonly FitCoachDbContext _db;
    private readonly AuthService _sut;
    private readonly string SenhaPadraoFaker;
    
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
        
        SenhaPadraoFaker = BCrypt.Net.BCrypt.HashPassword("senha123");
    }

    /// <summary>
    /// Nomenclatura Padrão: Método_Cenário_ResultadoEsperado
    /// </summary>
    [Fact]
    public async Task LoginAsync_CredenciaisCorretas_RetornaToken()
    {
        //Arrange
        var user = UserFaker.Default()
            .RuleFor(u => u.Email, "trainer@teste.com")
            .Generate();
        
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        
        //Act
        var resultado = await _sut.LoginAsync(new LoginRequest("trainer@teste.com", "senha123"));

        //Assert
        resultado.Should().NotBeNull();
        resultado.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_SenhaErrada_RetornaNull()
    {
        //Arrange
        var user = UserFaker.Default()
            .RuleFor(u => u.PasswordHash, "treiner@teste.com")
            .Generate();
        
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        
        //Act
        var resultado = await _sut.LoginAsync(new LoginRequest("treiner@teste.com", "senha-errada"));

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_EmailNaoCadastrado_RetornaNull()
    {
        //Act
        var resultado = await _sut.LoginAsync(new LoginRequest("naoexiste@teste.com", "senha123"));

        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_TokenGerado_ContemClaimsCorretas()
    {
        //Arrange
        var user = UserFaker.Default()
            .RuleFor(u => u.Email, "trainer-claims@teste.com")
            .Generate();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var treiner = TreinerFake.Default(user.Id).Generate();
        _db.TrainerProfiles.Add(treiner);
        await _db.SaveChangesAsync();

        //Act
        var resultado = await _sut.LoginAsync(new LoginRequest("trainer-claims@teste.com", "senha123"));

        //Assert
        var token = new JwtSecurityTokenHandler().ReadJwtToken(resultado!.Token);
        token.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be("trainer-claims@teste.com");
        token.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("Trainer");
        token.Claims.First(c => c.Type == "profileId").Value.Should().Be(treiner.Id.ToString());
    }

    [Fact]
    public async Task LoginAsync_UsuarioInativo_RetornaNull()
    {
        //Arrange
        var user = UserFaker.Default()
            .RuleFor(u => u.IsActive, false)
            .Generate();
        
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        
        //Act
        var resultado = await _sut.LoginAsync(new LoginRequest("user@teste.com", "senha123"));
        
        //Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task RegisterTreinerAsync_EmailNovo_CriaUsuarioEPerfil()
    {
        //Arrange
        var request = new RegisterTrainerRequest("Treiner Teste", "treiner@teste.com", "senha123", "Musculação", "012345-G/SP");

        //Act
        var resultado = await _sut.RegisterTrainerAsync(request);

        //Assert
        resultado.Should().NotBeNull();
        resultado.Token.Should().NotBeNullOrEmpty();

        var user = await _db.Users
            .Include(u => u.TrainerProfile)
            .FirstOrDefaultAsync(u => u.Email == "treiner@teste.com");

        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.Trainer);
        user.TrainerProfile.Should().NotBeNull();
        user.TrainerProfile!.CrefNumber.Should().Be("012345-G/SP");
    }

    [Fact]
    public async Task RegisterTreinerAsync_EmailDuplicado_LancaExcecao()
    {
        //Arrange
        var email = "treiner@teste.com";
        var user = UserFaker.Default()
            .RuleFor(u => u.Email, email)
            .Generate();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var treiner = TreinerFake.Default(user.Id).Generate();
        _db.TrainerProfiles.Add(treiner);
        await _db.SaveChangesAsync();

        var request = new RegisterTrainerRequest("Outro Treiner", email, "senha123", "Crossfit", "999999-G/RJ");

        //Act
        var act = () => _sut.RegisterTrainerAsync(request);

        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RegisterStudentAsync_EmailNovo_CriaUsuarioEPerfilVinculadoAoTrainer()
    {
        //Arrange
        var trainerUser = UserFaker.Default().Generate();
        _db.Users.Add(trainerUser);
        await _db.SaveChangesAsync();

        var treiner = TreinerFake.Default(trainerUser.Id).Generate();
        _db.TrainerProfiles.Add(treiner);
        await _db.SaveChangesAsync();

        var request = new RegisterStudentRequest("Aluno Teste", "aluno@teste.com", "senha123", "CODE-INVITE");

        //Act
        var resultado = await _sut.RegisterStudentAsync(request, treiner.Id);

        //Assert
        resultado.Should().NotBeNull();
        resultado.User.Email.Should().Be("aluno@teste.com");

        var aluno = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.TrainerId == treiner.Id);
        aluno.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterStudentAsync_EmailDuplicado_LancaExcecao()
    {
        //Arrange
        var trainerUser = UserFaker.Default().Generate();
        _db.Users.Add(trainerUser);
        var alunoExistente = UserFaker.Default()
            .RuleFor(u => u.Email, "aluno@teste.com")
            .Generate();
        _db.Users.Add(alunoExistente);
        await _db.SaveChangesAsync();

        var treiner = TreinerFake.Default(trainerUser.Id).Generate();
        _db.TrainerProfiles.Add(treiner);
        await _db.SaveChangesAsync();

        var request = new RegisterStudentRequest("Aluno", "aluno@teste.com", "senha123", "CODE-INVITE");

        //Act
        var act = () => _sut.RegisterStudentAsync(request, treiner.Id);

        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RegisterStudentAsync_TrainerInexistente_LancaExcecao()
    {
        //Arrange
        var request = new RegisterStudentRequest("Aluno", "aluno-orfao@teste.com", "senha123", "CODE-INVITE");

        //Act
        var act = () => _sut.RegisterStudentAsync(request, Guid.NewGuid());

        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Trainer não encontrado*");
    }

    public void Dispose() => _db.Dispose();
}