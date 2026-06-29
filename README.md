# FitCoach — Sistema de Gestão de Treinos

Plataforma completa inspirada no App Treino (apptreino.com.br) para academias e personal trainers.

## Arquitetura

```
FitCoach/
├── FitCoach.API/          → Backend .NET 8 + PostgreSQL
├── FitCoach.Web/          → Painel Web (Next.js 14) — Professor
├── FitCoach.Android/      → App Android (Kotlin) — Aluno     [Fase 2]
└── FitCoach.WearOS/       → App Wear OS (Kotlin) — Relógio   [Fase 2]
```

## Stack

| Camada | Tecnologia |
|--------|-----------|
| API | .NET 8 · ASP.NET Core · Entity Framework Core |
| Banco | PostgreSQL 16 |
| Auth | JWT Bearer + BCrypt |
| Web | Next.js 14 · TypeScript · Tailwind CSS · TanStack Query |
| Android | Kotlin · Jetpack Compose · Retrofit |
| Wear OS | Kotlin · Wear Compose · Health Services API |
| Cloud | AWS (S3 para mídia) / Azure App Service |

## Início Rápido (Docker)

```bash
# Clone e suba tudo com um comando
docker-compose up -d

# API disponível em: http://localhost:5000
# Swagger:           http://localhost:5000/swagger
# Painel Web:        http://localhost:3000
```

## Sem Docker (desenvolvimento)

### API (.NET)

```bash
cd FitCoach.API
#Suba o container de banco de dados
docker compose up postgres
# Configure o banco no appsettings.json
# Rode as migrations
dotnet ef database update

# Inicie a API
dotnet run
```

### Web (Next.js)

```bash
cd FitCoach.Web
npm install

# Configure o .env.local
echo "NEXT_PUBLIC_API_URL=http://localhost:5000/api" > .env.local

npm run dev
```

## Variáveis de Ambiente (Produção)

### API
```
ConnectionStrings__DefaultConnection=Host=...;Database=fitcoach;...
Jwt__Key=<mínimo 32 chars, gerado aleatoriamente>
Jwt__Issuer=fitcoach-api
Jwt__Audience=fitcoach-clients
```

### Web
```
NEXT_PUBLIC_API_URL=https://api.seudominio.com/api
```

## Rotas da API

### Auth
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/auth/login | Login (trainer ou student) |
| POST | /api/auth/register/trainer | Cadastrar personal trainer |
| POST | /api/auth/register/student | Cadastrar aluno (requer JWT de trainer) |

### Trainer
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/trainer/dashboard | Dashboard do professor |
| GET | /api/students | Listar meus alunos |
| GET | /api/students/{id} | Detalhes do aluno |
| GET | /api/students/{id}/activity | Histórico de atividade |
| GET | /api/plans | Listar meus planos |
| POST | /api/plans | Criar plano de treino |
| GET | /api/plans/{id} | Detalhes do plano |
| GET/POST | /api/exercises | Biblioteca de exercícios |

### Student (App Android/Web)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/dashboard | Treino do dia + histórico |
| POST | /api/sessions/start | Iniciar sessão de treino |
| POST | /api/sessions/{id}/sets | Registrar série |
| POST | /api/sessions/{id}/finish | Finalizar treino |
| GET | /api/sessions/{id} | Detalhes da sessão |

## Deploy na AWS

```bash
# 1. RDS PostgreSQL (recomendado: db.t4g.micro para começar)
# 2. Elastic Beanstalk ou ECS para a API .NET
# 3. Amplify ou S3 + CloudFront para o Next.js
# 4. S3 + CloudFront para mídia (vídeos/imagens dos exercícios)

# Configurar SSL com ACM (AWS Certificate Manager) — gratuito
```

## Próximas Fases

- **Fase 2**: App Android (Kotlin) + App Wear OS — execução do treino com timer, FC, Wearable Sync
- **Fase 3**: Avaliação física com fotos, gráficos de evolução
- **Fase 4**: Notificações push (Firebase), lembretes de treino
- **Fase 5**: Assinatura digital, pagamentos (Stripe/Pagar.me), multi-academia


## Ajuda
Tipos de Commits

O commit possui os elementos estruturais abaixo (tipos), que informam a intenção do seu commit ao utilizador(a) de seu código.

    fix - Commits do tipo fix indicam que seu trecho de código commitado está solucionando um problema (bug fix), (se relaciona com o PATCH do versionamento semântico).

    feat- Commits do tipo feat indicam que seu trecho de código está incluindo um novo recurso (se relaciona com o MINOR do versionamento semântico).

    docs - Commits do tipo docs indicam que houveram mudanças na documentação, como por exemplo no Readme do seu repositório. (Não inclui alterações em código).

    style - Commits do tipo style indicam que houveram alterações referentes a formatações de código, semicolons, trailing spaces, lint... (Não inclui alterações em código).

    refactor - Commits do tipo refactor referem-se a mudanças devido a refatorações que não alterem sua funcionalidade, como por exemplo, uma alteração no formato como é processada determinada parte da tela, mas que manteve a mesma funcionalidade, ou melhorias de performance devido a um code review.

    build - Commits do tipo build são utilizados quando são realizadas modificações em arquivos de build e dependências.

    test - Commits do tipo test são utilizados quando são realizadas alterações em testes, seja criando, alterando ou excluindo testes unitários. (Não inclui alterações em código)

    chore - Commits do tipo chore indicam atualizações de tarefas de build, configurações de administrador, pacotes... como por exemplo adicionar um pacote no gitignore. (Não inclui alterações em código)
