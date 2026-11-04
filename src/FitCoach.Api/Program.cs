using FitCoach.Api.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok("FitCoach.Api is running."));

app.MapGet("/config", (IOptions<DatabaseOptions> options) => 
    Results.Ok(new { 
        ConnectionStringConfigurada = !string.IsNullOrWhiteSpace(options.Value.ConnectionString) 
    }));

app.Run();

/// <summary>
/// Classe parcial Program para permitir testes de integração.
/// </summary>
public partial class Program { }
