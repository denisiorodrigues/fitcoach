## FitCoach
Aplicação para gerenciamento de alunos e treinos para perssonal trainers.


# Configuração de ambiente
Configuração inicial para faze a aplicação rodar.

Requisitos mínimos.
* Docker
* Dotnet DEV Kit versão mínima: 10

## Coneção com banco de dados
```bash
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=nome_banco_de_dados;Username=usuario_do_banco_de_dados;Password=senha_do_banco_de_dados;" --project .\src\FitCoach.Api\

```