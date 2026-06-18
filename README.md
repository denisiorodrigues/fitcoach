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
