# FitCoach — Regras de Negócio e Funcionalidades

Dicionário de referência: o que cada campo significa, o que é obrigatório, o que
cada funcionalidade faz e quais são as regras de autorização. Complementa
[`architecture.md`](./architecture.md) (visão técnica), [`roadmap.md`](./roadmap.md)
(fases e pendências), [`requisitos.md`](./requisitos.md) (requisitos numerados e
rastreáveis), [`plano-de-negocio.md`](./plano-de-negocio.md) (produto e receita) e
[`gamificacao.md`](./gamificacao.md) (design da gamificação do aluno).

**Levantado em 25 ago 2026**, direto do código (Models, DTOs, Services,
Controllers) do branch `chore/monorepo-restructure` — não da documentação antiga.
Onde uma regra ainda não existe no código, está marcado como lacuna, não como se
já existisse.

---

## 1. Perfis de usuário

Dois papéis fixos (`UserRole`): **Trainer** e **Student**, definidos no cadastro e
gravados no token JWT (claims `role` + `profileId`). Um usuário nunca muda de
papel.

---

## 2. Cadastro de Treinador — `POST /api/auth/register/trainer`

Público (não exige login).

| Campo | Obrigatório? | Regra atual |
|---|---|---|
| `name` | Sim | string não vazia (só o binding automático do ASP.NET checa isso — sem tamanho mínimo nem validação de caracteres) |
| `email` | Sim | único **em todo o sistema** — não pode repetir nem com outro treinador, nem com um aluno. Normalizado pra minúsculas. Sem validação de formato (`"abc"` passaria hoje). |
| `password` | Sim | sem regra de tamanho mínimo ou força hoje — qualquer string não vazia passa. Guardada como hash BCrypt. |
| `specialty` | Não | texto livre, até 200 caracteres |
| `crefNumber` (registro no CREF) | Não | sem validação de formato — ❓ ver §11 |

**Resultado**: cria `User` (role=Trainer) + `TrainerProfile`, devolve JWT direto —
o treinador já entra logado, sem precisar logar de novo.

---

## 3. Cadastro de Aluno — dois fluxos

### 3a. Cadastro direto (hoje) — `POST /api/auth/register/student`

Exige token de Trainer — é o treinador que preenche e envia, o aluno não
participa.

| Campo | Obrigatório? | Regra atual |
|---|---|---|
| `name` | Sim | idem treinador |
| `email` | Sim | único no sistema todo (mesma regra do treinador) |
| `password` | Sim | quem digita é o treinador — hoje não há nenhum mecanismo pra avisar o aluno dessa senha. ❓ ver §11 |
| `trainerInviteCode` | Presente no payload, mas **ignorado** | o vínculo com o treinador vem do token JWT de quem chama, não desse campo. Será substituído pelo fluxo de convite real (`roadmap.md`, Fase 1 item 3). |

**Resultado**: cria `User` (role=Student) + `StudentProfile` vinculado ao
`TrainerId` do token, devolve JWT direto (auto-login).

### 3b. Autocadastro via convite — planejado (`roadmap.md`, Fase 1 item 3)

Ainda não implementado. Regras já decididas no planejamento:
- Convite único por aluno (link/código de uso único, com validade — sugestão 7 dias)
- Convive com o cadastro direto (3a), não o substitui
- Aluno preenche os próprios dados: nome, e-mail, senha, **CPF** (novo campo, §4)
- Vínculo com o treinador vem do convite, não de um token

---

## 4. Perfil complementar do aluno (`StudentProfile`)

| Campo | Obrigatório? | Regra atual |
|---|---|---|
| `birthDate` | Não | — |
| `weightKg` | Não | — |
| `heightCm` | Não | — |
| `goal` (objetivo) | Não | até 300 caracteres |
| `healthNotes` | Não | até 500 caracteres — dado sensível de saúde, ver `architecture.md §6` |
| `cpf` (novo — `roadmap.md`, Fase 1 item 7) | **Vai virar obrigatório** | único **por treinador** (mesmo CPF pode existir sob treinadores diferentes) |

⚠️ **Lacuna encontrada agora, ainda fora do roadmap**: existe um DTO pronto
(`UpdateStudentProfileRequest`) pra editar esses campos, mas **nenhum endpoint o
usa** — hoje não há nenhuma forma de editar o perfil complementar de um aluno
depois do cadastro (preencher peso/altura/objetivo, por exemplo). Sugiro somar
isso à Fase 1 do roadmap.

---

## 5. Login — `POST /api/auth/login`

Público. `email` + `password`. Falha (`401`) se: e-mail não existe, senha errada,
ou usuário está com `isActive = false`. JWT válido por 7 dias + refresh token.

---

## 6. Exercícios

| Campo | Obrigatório? | Regra atual |
|---|---|---|
| `name` | Sim | até 150 caracteres |
| `muscleGroup` | Sim | um de 10 valores fixos (Chest, Back, Shoulders, Biceps, Triceps, Legs, Glutes, Core, FullBody, Cardio) — valor inválido → `400` |
| `equipment` | Sim | um de 8 valores fixos (Barbell, Dumbbell, Cable, Machine, Bodyweight, ResistanceBand, Kettlebell, Other) — valor inválido → `400` |
| `instructions` | Não | até 1000 caracteres |
| `videoUrl`, `thumbnailUrl` | Não | sem validação de URL |

Visibilidade: um exercício aparece pro treinador que o criou (`isGlobal=false`)
**ou** se for global (`isGlobal=true`, visível pra todos). Só treinador cria.

⚠️ **Gap já no roadmap** (Fase 1, item 2): a "biblioteca padrão" de exercícios
globais nunca foi semeada de fato no banco — hoje `isGlobal` nunca é `true` na
prática.

---

## 7. Plano de treino (Workout Plan)

Estrutura: Plano → Dias (`WorkoutDay`) → Exercícios do dia (`PlanExercise`).

**Plano**

| Campo | Obrigatório? | Regra |
|---|---|---|
| `studentId` | Sim | precisa pertencer ao treinador autenticado — senão `400` |
| `name` | Sim | até 150 caracteres |
| `description` | Não | até 500 caracteres |
| `startDate` / `endDate` | Não | — |
| `days` | Sim (lista) | pode ser vazia? sem validação hoje — ❓ ver §11 |

**Dia**

| Campo | Obrigatório? | Regra |
|---|---|---|
| `dayOfWeek` | Sim | enum padrão .NET (Sunday–Saturday) |
| `label` | Sim | ex: "Treino A", até 60 caracteres |
| `notes` | Não | até 200 caracteres |
| `orderIndex` | Sim | inteiro, sem validação de unicidade/sequência |

**Exercício do dia**

| Campo | Obrigatório? | Regra |
|---|---|---|
| `exerciseId` | Sim | precisa existir |
| `sets` | Sim | inteiro, sem mínimo validado |
| `reps` | Sim | texto livre (ex: "8-12"), até 20 caracteres — não é número, é faixa |
| `weightKg` | Não | — |
| `restSeconds` | Sim | inteiro |
| `coachNotes` | Não | até 300 caracteres |

Autorização: treinador só vê plano que ele criou; aluno só vê plano em que ele é
o `studentId`. Fora disso → `404` (nunca revela que o recurso existe — padrão
adotado em todo o sistema pra evitar IDOR).

⚠️ **Gap já no roadmap** (Fase 1, item 1): `PUT /api/plans/{id}` não existe —
plano não pode ser editado depois de criado, mesmo já existindo um DTO pronto
(`UpdateWorkoutPlanRequest`) pra isso.

---

## 8. Execução de treino (Sessão)

Só o papel Student acessa.

| Ação | Campos | Obrigatório | Regra |
|---|---|---|---|
| Iniciar sessão | `workoutDayId` | Sim | grava `startedAt` automaticamente |
| Registrar série | `planExerciseId`, `setNumber`, `repsDone`, `weightKg` | Sim (todos) | só na sessão do próprio aluno — sessão de outro aluno → `404` |
| Finalizar sessão | `avgHeartRate`, `caloriesBurned`, `studentNotes` | Não (todos opcionais) | calcula `durationSeconds` automaticamente |

Dashboard do aluno calcula: treino do dia (baseado no `dayOfWeek` atual x plano
ativo), próximo treino, recordes pessoais (maior peso por exercício), contadores
de sessões (mês / total).

---

## 9. Regra transversal de autorização (vale pra tudo)

- Sem token → `401`.
- Token com papel errado (ex: Student tentando acessar rota de Trainer) → `403`.
- Tentar acessar/alterar recurso de **outro** treinador/aluno → `404` (nunca
  `403` — não revela que o recurso existe).

---

## 10. Lacunas de validação (não é bug, é ausência de regra)

O projeto tem a dependência `FluentValidation.AspNetCore` instalada, mas **nunca
foi configurada nem usada** (nenhum validator existe, nada é registrado no
`Program.cs`). Hoje a única validação que existe é:
- o que o EF Core aplica (`MaxLength`, tipos);
- o que o ASP.NET Core aplica automaticamente pra propriedades não-anuláveis —
  só rejeita `null`, não rejeita string vazia (`""`) nem formato inválido.

Não existe validação de: formato de e-mail, força de senha, tamanho mínimo de
nome, lista de dias vazia num plano. Registrado aqui pra decidir se/quando isso
entra no roadmap.

---

## 11. Perguntas em aberto (decisões do PO)

- ❓ **CREF obrigatório?** Hoje é opcional. Num produto pra personal trainers,
  cadastro sem CREF pode ser um problema de credibilidade/legal — vale exigir,
  ou manter opcional (treinadores autônomos sem registro)?
- ❓ **Senha do aluno no cadastro direto**: hoje o treinador digita a senha do
  aluno na hora do cadastro. Como o aluno fica sabendo dela? Fica por fora do
  sistema (o treinador informa por WhatsApp etc.), ou isso devia gerar uma senha
  temporária + fluxo de "primeiro acesso, troque sua senha"?
- ❓ **Regras de senha/e-mail**: definir tamanho mínimo de senha e se o formato
  de e-mail deve ser validado no backend.
- ❓ **Plano sem nenhum dia**: deveria ser permitido criar um plano com `days`
  vazio, ou isso devia virar erro (`400`)?

---

## Changelog

- **25 ago 2026**: criação, levantado direto do código (Models, DTOs, Services,
  Controllers) do branch `chore/monorepo-restructure`.
