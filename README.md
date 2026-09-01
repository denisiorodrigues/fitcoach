# FitCoach — Sistema de Gestão de Treinos

Plataforma completa inspirada no App Treino (apptreino.com.br) para academias e personal trainers.

## Documentação

| Documento | Conteúdo |
|---|---|
| [`docs/plano-de-negocio.md`](docs/plano-de-negocio.md) | Visão de produto, personas, concorrência, modelo de receita e riscos |
| [`docs/requisitos.md`](docs/requisitos.md) | Requisitos funcionais (RF) e não-funcionais (RNF) numerados, com prioridade, status e rastreabilidade |
| [`docs/gamificacao.md`](docs/gamificacao.md) | Design da gamificação do aluno — streak, dias treinados, conquistas e ranking |
| [`docs/roadmap.md`](docs/roadmap.md) | Fases de implementação e pendências para fechar cada uma |
| [`docs/regras-de-negocio.md`](docs/regras-de-negocio.md) | Dicionário de campos, obrigatoriedades e regras de autorização |
| [`docs/architecture.md`](docs/architecture.md) | Visão técnica e decisões de arquitetura |
| [`docs/manual-do-usuario.md`](docs/manual-do-usuario.md) | Como usar cada tela |

## Arquitetura

```
FitCoach/
├── apps/
│   ├── backend/           → API .NET 10 + PostgreSQL
│   ├── web/               → Painel Web (Next.js 14) — Treinador
│   └── mobile/            → App do aluno (React Native)        [Fase 3]
└── packages/              → Código compartilhado (watch-shared/KMP) [Fase 4]
```

## Stack

| Camada | Tecnologia                                              |
|--------|---------------------------------------------------------|
| API | .NET 10 · ASP.NET Core · Entity Framework Core          |
| Banco | PostgreSQL 16                                           |
| Auth | JWT Bearer + BCrypt                                     |
| Web | Next.js 14 · TypeScript · Tailwind CSS · TanStack Query |
| Phone (aluno) | React Native                                      |
| Watch — lógica | Kotlin Multiplatform (módulo `watch-shared`)     |
| Watch — UI | SwiftUI (watchOS) · Jetpack Compose (Wear OS)         |
| Hospedagem | VPS Hostinger no início; migração pra nuvem de mercado (AWS/Azure/GCP) depois — ver `docs/roadmap.md` |

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
cd apps/backend/FitCoach.API
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
cd apps/web
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

### Student (app do aluno)
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/dashboard | Treino do dia + histórico |
| POST | /api/sessions/start | Iniciar sessão de treino |
| POST | /api/sessions/{id}/sets | Registrar série |
| POST | /api/sessions/{id}/finish | Finalizar treino |
| GET | /api/sessions/{id} | Detalhes da sessão |

## Deploy

**Decidido em 31 ago 2026**: o projeto começa hospedado numa **VPS da Hostinger**
(restrição de orçamento), com migração planejada pra uma nuvem de mercado (AWS,
Azure ou GCP) depois. Vale tanto pra API/banco quanto pro armazenamento de mídia
(fotos e vídeos da avaliação física).

O desenho de infra ainda não está fechado — banco gerenciado ou na própria VPS,
forma de armazenar mídia, limites de arquivo e o desenho da migração estão
listados em `docs/architecture.md` §7 e em `docs/roadmap.md` ("Fora de fase").

Esboço antigo de deploy 100% AWS (RDS + Elastic Beanstalk/ECS + Amplify/S3 +
CloudFront + ACM) fica registrado como referência pra fase de migração, não como
o plano atual.

## Próximas Fases

Resumo — o detalhamento por fase, com critérios de "pronto", está em
`docs/roadmap.md`.

- **Fase 1** — Backend (API .NET): fechar gaps funcionais, CI/CD e itens de segurança
- **Fase 2** — Painel web do treinador (Next.js): login, alunos, edição de plano
- **Fase 3** — App do aluno (React Native): treino do dia, execução, histórico
- **Fase 4** — Relógio (watchOS + Wear OS via módulo KMP)
- **Backlog** — Avaliação física (anamnese, medidas, fotos/vídeos), gamificação,
  notificações push, pagamentos e multi-academia


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
