using System.ComponentModel.DataAnnotations;

namespace FitCoach.Api.Configuration;

/// <summary>
/// Representa as opções de configuração do banco de dados.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// O nome da seção de configuração no arquivo appsettings.json ou em variáveis de ambiente.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Obtém ou define a string de conexão com o banco de dados.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Database:ConnectionString não foi configurada. " +
                       "Em desenvolvimento use: dotnet user-secrets set \"Database:ConnectionString\" \"...\"")]
    public string  ConnectionString { get; set; } = string.Empty;
}
