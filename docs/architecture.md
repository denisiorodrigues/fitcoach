# FitCoach — Documento de Arquitetura

**Escopo**: FitCoach.API + FitCoach.Web + mobile (planejado)
**Baseado em**: documentação gerada em 20 ago 2026 (branch `test/integration-controllers`) + decisões de arquitetura discutidas para as próximas fases
**Autor**: mantido por um desenvolvedor solo
**Ver também**: [`requisitos.md`](./requisitos.md) (requisitos funcionais e não-funcionais numerados), [`roadmap.md`](./roadmap.md) (fases e pendências), [`regras-de-negocio.md`](./regras-de-negocio.md), [`plano-de-negocio.md`](./plano-de-negocio.md) (produto e receita) e [`gamificacao.md`](./gamificacao.md) (design da gamificação do aluno)

---

## 1. Visão geral do produto

FitCoach é um sistema de gestão de treinos para personal trainers: o treinador monta
planos de treino por dia da semana (exercícios, séries, repetições, carga,
descanso) e acompanha a atividade de cada aluno. O aluno executa o treino e
registra cada série.

Dois perfis fixos, definidos no cadastro e gravados no token JWT junto com o
`profileId`:

- **Trainer**: cadastra alunos vinculados a si, monta planos, acompanha dashboard.
- **Student**: vinculado a exatamente um treinador; consulta treino do dia, executa
  sessão, registra séries, vê recordes e histórico.

## 2. Estado atual (o que existe de fato)

| Componente | Status |
|---|---|
| `FitCoach.API` | Funcionando — .NET 10, ASP.NET Core, EF Core, JWT + BCrypt |
| `FitCoach.Web` | Parcial — Next.js 14/TS/Tailwind/TanStack Query. Só 2 telas reais: Dashboard do treinador e Novo plano de treino. Login, lista de alunos, detalhe de aluno e edição de plano aparecem na UI mas **não estão implementados** |
| Banco | PostgreSQL 16, migrations automáticas na subida da API (exceto testes de integração, que usam banco em memória) |
| App do aluno (mobile) | **Não existe** — nem web nem mobile. Endpoints de dashboard/sessão já funcionam, só acessíveis via Swagger/cliente HTTP |
| Convite de aluno por código | Campo `trainerInviteCode` existe no request mas é **ignorado** pelo backend — vínculo é sempre com o treinador autenticado que faz a chamada |
| Edição de plano | Web já chama `PUT /api/plans/{id}`, mas **esse endpoint não existe** na API ainda |

Empacotamento via Docker Compose (`postgres`, `api`, `web`). API em `localhost:5000`
(Swagger em `/swagger`), painel em `localhost:3000`.

**Pendência de segurança antes de publicar o painel web publicamente**: a política
de CORS na API hoje libera só `localhost:3000` e um domínio placeholder
(`fitcoach.yourdomain.com`) — precisa ajustar para o domínio real antes de expor o
painel fora da rede local. A documentação de uso pode ir ao ar independente disso,
já que não expõe a API em si.

## 3. Modelo de dados

Três domínios: identidade, prescrição, execução.

**Identidade**
- `User` — name, email, passwordHash, role, avatarUrl
- `TrainerProfile` — bio, specialty, crefNumber (1—N `StudentProfile`, `Exercise`, `WorkoutPlan`)
- `StudentProfile` — birthDate, weightKg, heightCm, goal, healthNotes (N—1 `TrainerProfile`)

**Prescrição**
- `Exercise` — name, muscleGroup, equipment, isGlobal
- `WorkoutPlan` — name, description, startDate, endDate, isActive (1—N `WorkoutDay`)
- `WorkoutDay` — dayOfWeek, label, orderIndex (1—N `PlanExercise`)
- `PlanExercise` — sets, reps, weightKg, restSeconds, coachNotes (N—1 `Exercise`)

**Execução**
- `WorkoutSession` — startedAt, finishedAt, avgHeartRate, caloriesBurned, durationSeconds (N—1 `WorkoutDay`, 1—N `SessionSet`)
- `SessionSet` — setNumber, repsDone, weightKg, loggedAt (N—1 `PlanExercise`)

Grupos musculares: Chest, Back, Shoulders, Biceps, Triceps, Legs, Glutes, Core,
FullBody, Cardio. Equipamentos: Barbell, Dumbbell, Cable, Machine, Bodyweight,
ResistanceBand, Kettlebell, Other.

> Nota de arquitetura: `StudentProfile.healthNotes` e os dados de `WorkoutSession`
> (frequência cardíaca, calorias) são dados sensíveis de saúde/atividade física —
> isso é referenciado na seção 6 (segurança) e vale como critério de auditoria na
> skill `code-audit` já criada para este projeto.

## 4. Referência da API (resumo)

Base `/api`. Autenticação via JWT Bearer, 7 dias de validade, com refresh token.
Hoje o `FitCoach.Web` guarda ambos no `localStorage`.

| Método | Rota | Quem acessa | O que faz |
|---|---|---|---|
| POST | `/auth/login` | Público | Autentica, devolve JWT + refresh token |
| POST | `/auth/register/trainer` | Público | Autocadastro de treinador |
| POST | `/auth/register/student` | Trainer | Cadastra aluno vinculado ao treinador logado |
| GET | `/trainer/dashboard` | Trainer | Contadores gerais + atividade recente |
| GET | `/students` | Trainer | Lista alunos do treinador logado |
| GET | `/students/{id}` | Trainer | Detalhe de um aluno |
| GET | `/students/{id}/activity` | Trainer | Últimas 10 sessões + contadores do mês |
| GET | `/exercises` | Autenticado | Biblioteca de exercícios (filtra por muscle/equipment) |
| POST | `/exercises` | Trainer | Cria exercício próprio |
| GET | `/plans` | Trainer | Lista planos criados |
| GET | `/plans/{id}` | Trainer dono ou Student dono | Detalhe completo do plano |
| POST | `/plans` | Trainer | Cria plano completo (dias + exercícios) em uma chamada |
| GET | `/dashboard` | Student | Treino do dia, próximo treino, recordes, sessões recentes |
| POST | `/sessions/start` | Student | Inicia sessão de treino |
| GET | `/sessions/{id}` | Student | Detalhe de sessão com séries |
| POST | `/sessions/{id}/sets` | Student | Registra série (peso e repetições) |
| POST | `/sessions/{id}/finish` | Student | Encerra sessão com FC média, calorias, notas |

Endpoints referenciados pelo client mas **inexistentes hoje**: `PUT /api/plans/{id}`.

## 5. Arquitetura alvo (próximas fases)

O README já previa fases futuras; as decisões abaixo foram tomadas para a fase de
app mobile (Fase 2: app Android/iOS + Wear OS para o aluno treinar com timer e
frequência cardíaca).

### 5.1 Contexto da decisão

- Lógica de negócio de treino/prescrição continua centralizada na API (.NET) —
  os clientes (web, phone, watch) são majoritariamente consumidores.
- **Exceção**: tracking de sessão de treino no relógio precisa funcionar **offline**
  (cache local) e sincronizar quando a conexão voltar — essa é lógica real que
  precisa rodar no dispositivo, não só na API.
- Watch (watchOS e Wear OS) exige UI nativa — não há suporte maduro a React Native
  nem Flutter para watch.
- Desenvolvedor solo, aprendendo as stacks de mobile do zero.

### 5.2 Stack escolhida por camada

| Camada | Tecnologia | Motivo |
|---|---|---|
| Backend | .NET 10 (já existente) | Mantido — sem mudança |
| Web (treinador) | Next.js 14 (já existente) | Mantido — sem mudança |
| Phone (aluno) | React Native | Reaproveita conhecimento de React do web; app phone é majoritariamente consumidor de API, não precisa de lógica standalone pesada |
| Watch — lógica (tracking, cache, sync) | **Kotlin Multiplatform (KMP)**, módulo `shared` compilado para watchOS (Kotlin/Native) e Wear OS | É a única lógica que **precisa** rodar igual nos dois relógios sem depender de conexão; evita duplicar bug de sync entre Swift e Kotlin |
| Watch — UI iOS | Swift/SwiftUI nativo | UI nativa do watchOS, consumindo o módulo `shared` |
| Watch — UI Wear OS | Kotlin/Jetpack Compose nativo | UI nativa do Wear OS, consumindo o módulo `shared` |

Decisão explícita: **não usar Compose Multiplatform para UI** — o módulo `shared`
em KMP cobre só a lógica de tracking/cache/sync; a UI de cada watch é nativa
(SwiftUI de um lado, Compose "normal" do outro). Isso evita depender da parte mais
nova/instável do ecossistema KMP (Compose Multiplatform para iOS) numa área onde
não há ganho real de compartilhamento — a UI já é bem diferente entre as duas
plataformas de qualquer forma.

### 5.3 Estrutura de pastas proposta (monorepo)

```
fitcoach/
├── apps/
│   ├── backend/                 # FitCoach.API — .NET 10 (já existente)
│   ├── web/                     # FitCoach.Web — Next.js 14 (já existente)
│   ├── mobile/
│   │   ├── phone/                # React Native — app do aluno
│   │   ├── watch-shared/         # módulo KMP: tracking, cache local, sync
│   │   │   └── src/
│   │   │       ├── commonMain/   # lógica de sessão, modelos, regras de sync
│   │   │       ├── iosMain/      # expect/actual específico watchOS
│   │   │       └── androidMain/  # expect/actual específico Wear OS
│   │   ├── watch-ios/            # projeto Xcode + SwiftUI, consome watch-shared
│   │   └── watch-wear/           # projeto Android + Compose, consome watch-shared
│   
├── packages/
│   └── config/                   # eslint, tsconfig — compartilhado entre web e phone (JS/TS)
├── docs/
│   └── architecture.md           # este documento
├── .audit/                       # relatórios gerados pela skill code-audit
├── .github/
│   └── workflows/
└── README.md
```

### 5.4 Fronteira de dados phone ↔ watch ↔ API

- O relógio registra a sessão de treino (séries, FC) **localmente**, mesmo sem
  conexão com o phone ou internet — módulo `watch-shared` cuida do cache.
- Quando há conexão (com o phone via Bluetooth, ou direto com a API se o watch
  tiver conectividade própria), os dados são sincronizados. É preciso decidir e
  documentar depois: o relógio sincroniza direto com a API, ou sempre via phone
  como "ponte"? Isso ainda não foi definido nesta conversa e deveria virar uma
  decisão registrada aqui antes da implementação.
- O phone (React Native) não precisa reimplementar a lógica de tracking — ele é
  majoritariamente consumidor da API para o que não vem do relógio (ex: consultar
  plano do dia, ver histórico).

## 6. Segurança — pontos já identificados

- **CORS da API** libera hoje só `localhost:3000` e `fitcoach.yourdomain.com` —
  ajustar para o domínio real antes de expor o painel web publicamente (bloqueante
  para publicação, não para a documentação em si).
- **Tokens no `localStorage`** no `FitCoach.Web` — vale reavaliar para cookie
  `httpOnly` + `Secure` + `SameSite` quando o painel for exposto publicamente,
  para reduzir superfície de XSS.
- **Dados sensíveis de saúde/atividade física** (`healthNotes`, FC, calorias) —
  ao construir os apps mobile, aplicar os mesmos cuidados já definidos na skill de
  auditoria: cache local com expiração, sem dado sensível em log, tráfego sempre
  em TLS.
- **`trainerInviteCode` ignorado pelo backend** — não é falha de segurança em si
  (o vínculo hoje é sempre pelo token autenticado, que é mais seguro que um código
  público), mas é uma inconsistência entre o que o client sugere e o que a API faz
  — vale corrigir ou remover o campo do client até o backend suportar de fato.
- Uma auditoria completa (segurança + qualidade) pode ser rodada a qualquer momento
  usando a skill `code-audit`, já configurada para as stacks deste projeto
  (.NET, React/Next.js, React Native, Swift, Kotlin/KMP).

## 7. Lacunas conhecidas / próximos passos

Da documentação atual do repositório:
- Implementar telas faltantes do `FitCoach.Web`: login, lista de alunos, detalhe de
  aluno, edição de plano.
- Implementar `PUT /api/plans/{id}` na API (já referenciado pelo client).
- Decidir se `trainerInviteCode` vira fluxo real de autocadastro por convite, ou é
  removido do client.

Das decisões de arquitetura mobile (a confirmar antes de começar a implementação):
- Definir se o watch sincroniza direto com a API ou sempre via phone.
- Definir formato de persistência local do módulo `watch-shared` (arquivo,
  SQLite via KMP, etc.).
- Ajustar CORS para domínio real antes de publicar o painel web.

Hospedagem/infraestrutura de produção (decisão de 31 ago 2026, `roadmap.md`
"Fora de fase"): começa em **VPS da Hostinger** (restrição de orçamento), com
migração planejada para nuvem de mercado (AWS, Azure ou GCP) depois — vale tanto
para a API/banco quanto para o armazenamento de fotos/vídeos de avaliação física
(`requisitos.md` §9, RF-AVA-10). Diverge do esboço de deploy 100% AWS que hoje
está no README; ainda a definir: banco gerenciado ou na própria VPS, forma de
armazenar mídia na Hostinger (disco local vs. object storage), o limite de
tamanho/duração de arquivo pra fotos/vídeos (`requisitos.md` §13, decisão
técnica em aberto de propósito) e o desenho da migração para a nuvem de
mercado quando chegar a hora.
