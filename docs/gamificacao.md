# FitCoach — Design de Gamificação (aluno)

Design técnico da gamificação do aluno: sequência de treinos (streak), contador de
dias treinados, conquistas e ranking entre alunos do mesmo treinador. Complementa:

- [`requisitos.md`](./requisitos.md) §10 (módulo RF-GAM) — requisitos numerados;
- [`roadmap.md`](./roadmap.md) — em que ponto entra no faseamento;
- [`plano-de-negocio.md`](./plano-de-negocio.md) — por que (retenção do aluno);
- [`regras-de-negocio.md`](./regras-de-negocio.md) §7–§8 — plano e sessão, que a
  gamificação consome;
- [`architecture.md`](./architecture.md) §3 — modelo de dados atual.

**Escrito em 27 ago 2026.** É um documento de design — nada abaixo está
implementado. Escopo definido com o dono do projeto: streak + dias treinados +
conquistas + ranking, com a regra de streak baseada em **dia prescrito
concluído**. Decisões finas em aberto estão na §9.

---

## 1. Objetivo

Aumentar a adesão do aluno recompensando constância. O aluno vê:

- **Sequência atual** e **melhor sequência** (treinos prescritos concluídos em dias
  seguidos, sem furar);
- **Dias treinados** no mês e no acumulado;
- **Conquistas** desbloqueadas e progresso para as próximas;
- **Ranking** entre os alunos do mesmo treinador.

Tudo é derivado do que a API já registra hoje (`WorkoutSession`) — a gamificação
não muda o fluxo de treino, só lê o histórico.

---

## 2. O que é reaproveitado

| Entidade existente | Campo usado | Para quê |
|---|---|---|
| `WorkoutSession` (`regras-de-negocio.md §8`) | `startedAt`, `finishedAt`, `WorkoutDayId` | Identificar quando o aluno concluiu um treino |
| `WorkoutDay` (`§7`) | `dayOfWeek` | Saber que dias da semana o plano prescreve |
| `WorkoutPlan` (`§7`) | `isActive`, `startDate`, `endDate`, `studentId` | Saber qual plano vale em cada data |
| `StudentProfile` (`§4`) | `Id` | Dono da gamificação |

O dashboard do aluno (`GET /api/dashboard`) **já calcula** contadores de sessões
(mês / total) e recordes de carga — a gamificação estende esse retorno, não o
duplica.

---

## 3. Modelo de dados novo

Três entidades novas + uma migration. Nenhuma mudança em `WorkoutSession` /
`WorkoutDay`.

### 3.1 `StudentGamification` (1–1 com `StudentProfile`)

Snapshot em cache do estado do aluno — recalculado nos gatilhos da §5, nunca é a
fonte de verdade (a fonte é o histórico de sessões + planos).

| Campo | Tipo | Descrição |
|---|---|---|
| `StudentProfileId` | Guid (PK, FK) | Dono |
| `CurrentStreak` | int | Ocorrências prescritas consecutivas concluídas até ontem |
| `LongestStreak` | int | Maior sequência já atingida |
| `StreakEvaluatedThrough` | date | Última data (fuso do aluno) já avaliada pelo recompute |
| `TotalTrainedDays` | int | Total de dias com pelo menos uma sessão concluída |
| `LastTrainedDate` | date? | Data da última sessão concluída |
| `PointsTotal` | int | Soma de pontos (§7) |
| `OptOutLeaderboard` | bool | Aluno fora do ranking (§8) |

### 3.2 `Achievement` (catálogo, semeado via `HasData`)

| Campo | Tipo | Descrição |
|---|---|---|
| `Code` | string (PK) | Ex.: `STREAK_7` |
| `Title` | string | "7 dias em sequência" |
| `Description` | string | Texto curto |
| `Category` | enum | `FirstStep`, `Streak`, `Volume`, `PerfectWeek`, `Comeback` |
| `Threshold` | int? | Valor-alvo (streak ou dias), quando aplicável |
| `Points` | int | Pontos ganhos ao desbloquear |
| `IconKey` | string | Chave de ícone para o client |

### 3.3 `StudentAchievement`

| Campo | Tipo | Descrição |
|---|---|---|
| `StudentProfileId` | Guid (FK) | Aluno |
| `AchievementCode` | string (FK) | Conquista |
| `UnlockedAt` | datetime (UTC) | Quando desbloqueou |

Índice único `(StudentProfileId, AchievementCode)` — cada conquista desbloqueia
uma vez.

> **Decisão de MVP**: não haverá tabela de log de eventos (`GamificationEvent`).
> O recompute determinístico da §5 é suficiente e mais simples. Um log pode entrar
> depois se for preciso auditar ou alimentar um feed de atividade.

---

## 4. Regra da sequência (streak) — "dia prescrito concluído"

### 4.1 Definições

- **Plano ativo numa data `D`**: `WorkoutPlan` do aluno com `isActive = true` e,
  se `startDate`/`endDate` estiverem preenchidos, `D` dentro do intervalo. Se
  houver mais de um (não deveria), usa o de `startDate` mais recente.
- **Ocorrência prescrita**: par `(D, WorkoutDay)` em que `D.DayOfWeek` casa com o
  `dayOfWeek` de um `WorkoutDay` do plano ativo em `D`.
- **Data de referência de uma sessão**: a data de `startedAt` convertida para o
  fuso do aluno. A sessão pode ser finalizada (`finishedAt`) até **24h** depois e
  ainda contar para essa data (tolerância para quem esquece de encerrar).
- **Ocorrência concluída**: existe `WorkoutSession` ligada àquele `WorkoutDay` com
  `finishedAt != null` e data de referência `= D`.
- **Ocorrência perdida**: `D` já venceu (passou o fim do dia no fuso do aluno) e
  nenhuma sessão a concluiu.

### 4.2 Cálculo

- **Sequência atual** = quantidade de ocorrências prescritas consecutivas
  concluídas, contando da ocorrência **já vencida** mais recente para trás, até a
  primeira lacuna.
- Uma **ocorrência perdida zera** a sequência a partir dali.
- A **ocorrência de hoje ainda em aberto** não conta nem quebra — o client mostra
  o valor consolidado + um selo "treino de hoje pendente".
- **Melhor sequência** = maior valor que a sequência atual já teve (atualizado no
  recompute).

### 4.3 Casos de borda (todos decididos aqui)

| Caso | Comportamento |
|---|---|
| **Sem plano ativo** numa data | Não há ocorrência prescrita → sequência **pausa** (mantém o valor, `StreakEvaluatedThrough` avança). Ver §9 pergunta 2. |
| **Plano criado com `startDate` no passado** | Ocorrências anteriores a `max(startDate, plano.createdAt)` **não** contam como perdidas. A sequência vale da ativação em diante. |
| **Troca de plano no meio da semana** | Cada data usa o plano ativo naquela data. Ocorrências do plano antigo após a troca deixam de existir. |
| **Dois `WorkoutDay` no mesmo `dayOfWeek`** (plano mal montado) | A data conta como concluída se **qualquer** um foi concluído; perdida só se nenhum foi. |
| **Mais de uma sessão no mesmo dia prescrito** | Conta como uma ocorrência concluída (não soma). |
| **Sessão iniciada e nunca finalizada** | Não conclui a ocorrência. Depois de 24h de `startedAt`, é ignorada pelo cálculo. |
| **Fuso horário** | MVP: fixo `America/Sao_Paulo`. Campo de fuso por aluno é item futuro (§9 pergunta 1). |

---

## 5. Recompute (sem job agendado no MVP)

`StudentGamification` é recalculado em **dois gatilhos**:

1. **Ao finalizar uma sessão** (`POST /api/sessions/{id}/finish`);
2. **Na leitura** de `GET /api/gamification/me`, `GET /api/dashboard` e
   `GET /api/students/{id}/gamification`.

Algoritmo:

1. Carrega o snapshot. Se não existe, cria zerado com
   `StreakEvaluatedThrough = ativação do primeiro plano do aluno − 1 dia`.
2. Enumera as **ocorrências prescritas** entre `StreakEvaluatedThrough + 1` e
   **ontem** (fuso do aluno), em ordem cronológica.
3. Para cada ocorrência: se concluída, `CurrentStreak++` e
   `LongestStreak = max(LongestStreak, CurrentStreak)`; se perdida,
   `CurrentStreak = 0`.
4. Recalcula `TotalTrainedDays`, `LastTrainedDate`, `PointsTotal` (§7) e avalia
   conquistas (§6).
5. `StreakEvaluatedThrough = ontem`. Persiste o snapshot.

Propriedades:

- **Determinístico e idempotente** — rodar duas vezes dá o mesmo resultado.
- **Custo limitado** — no máximo ~7 ocorrências por semana; cap de janela de
  **400 ocorrências** (~4 anos) por segurança.
- Um **job noturno** é melhoria futura: necessário para push do tipo "sua
  sequência acaba hoje" e para o ranking ficar fresco sem depender de leitura.

---

## 6. Conquistas — catálogo inicial

Semeado via `HasData` na migration. Pontos são **`[hipótese — validar]`**.

| Code | Categoria | Critério | Pontos |
|---|---|---|---|
| `FIRST_SESSION` | FirstStep | 1ª sessão concluída | 10 |
| `STREAK_3` | Streak | sequência ≥ 3 | 20 |
| `STREAK_7` | Streak | sequência ≥ 7 | 50 |
| `STREAK_14` | Streak | sequência ≥ 14 | 100 |
| `STREAK_30` | Streak | sequência ≥ 30 | 250 |
| `DAYS_10` | Volume | 10 dias treinados | 30 |
| `DAYS_50` | Volume | 50 dias treinados | 120 |
| `DAYS_100` | Volume | 100 dias treinados | 300 |
| `PERFECT_WEEK` | PerfectWeek | todas as ocorrências prescritas de uma semana concluídas | 40 |
| `COMEBACK` | Comeback | concluiu uma sessão após ≥ 14 dias sem treinar | 25 |

Desbloqueio: durante o recompute, para cada conquista ainda não em
`StudentAchievement`, testa o critério; se passou, insere com `UnlockedAt = agora`
e soma `Points` ao `PointsTotal`. Conquista nunca é revogada (sequência pode
cair, mas o `STREAK_7` já conquistado permanece).

---

## 7. Pontos

```
PointsTotal = 10 × (ocorrências prescritas concluídas)
            + Σ Points das conquistas desbloqueadas
```

- Fórmula deliberadamente simples; os números são `[hipótese — validar]`.
- **Sem penalidade** por perder um dia — o aluno só deixa de ganhar.
- Se os pontos entram ou não no ranking: §9 pergunta 7.

---

## 8. Ranking entre alunos do treinador

- `GET /api/gamification/leaderboard` — o aluno autenticado recebe o placar dos
  **alunos do mesmo treinador**.
- **Ordenação (MVP, a confirmar — §9 pergunta 5)**: `CurrentStreak` desc,
  desempate por dias treinados no mês.
- **Privacidade**:
  - No placar do aluno, os outros aparecem como **primeiro nome + inicial do
    sobrenome** (ex.: "Marina S.").
  - `OptOutLeaderboard = true` → o aluno não aparece para ninguém e não vê os
    outros (só a própria posição/estatística). Padrão do flag: §9 pergunta 4.
  - O **treinador** vê o placar completo, com nomes completos, no painel (estende
    a tela de detalhe do aluno / uma aba de turma).
- Nenhum dado sensível de saúde entra no ranking (só contadores de atividade).

---

## 9. Perguntas em aberto (decisão do PO)

| # | Questão | Sugestão |
|---|---|---|
| 1 | Fuso horário por aluno (campo `TimeZone` em `StudentProfile`) ou fixo `America/Sao_Paulo`? | Fixo no MVP; campo por aluno depois |
| 2 | Sem plano ativo: a sequência **pausa** (recomendado) ou **quebra** após N dias? | Pausa |
| 3 | Tolerância para finalizar a sessão: 24h a partir de `startedAt` — confirmar valor | 24h |
| 4 | `OptOutLeaderboard` começa ligado ou desligado? Aluno fora do ranking ainda vê a própria posição? | Desligado (participa); vê a própria posição |
| 5 | Métrica de ordenação do ranking: sequência, pontos ou dias no mês? | Sequência |
| 6 | "Semana perfeita": semana = segunda a domingo? Conta mesmo se o plano tem 6 dias prescritos? | Seg–dom; conta qualquer nº de dias prescritos |
| 7 | Pontos são só cosméticos ou entram como métrica de ranking? | Cosmético no MVP |
| 8 | "Congelar sequência" (streak freeze estilo Duolingo — 1 falha perdoada por período)? | Fora do MVP; registrar como possível |

---

## 10. Superfície de API nova

Base `/api`, JWT Bearer (mesmo padrão de `architecture.md §4`).

| Método | Rota | Quem | O que |
|---|---|---|---|
| GET | `/gamification/me` | Student | Sequência atual + melhor, dias treinados (mês/total), pontos, conquistas desbloqueadas + progresso das próximas |
| GET | `/gamification/leaderboard` | Student | Ranking dos alunos do treinador (respeita opt-out) |
| PUT | `/gamification/me/preferences` | Student | Alterna `OptOutLeaderboard` |
| GET | `/students/{id}/gamification` | Trainer dono | Visão da gamificação de um aluno (estende `GET /students/{id}` — `requisitos.md` RF-TRN-4) |

Além disso:

- `GET /api/dashboard` (Student) ganha um bloco `gamification` resumido
  (sequência, dias no mês, conquista mais recente).
- `GET /api/trainer/dashboard` **pode** ganhar um agregado ("aluno mais
  consistente", sequência média da turma) — fora do MVP.

Autorização segue a regra transversal (`regras-de-negocio.md §9`): aluno só vê a
própria gamificação e o ranking do próprio treinador; treinador só vê alunos da
própria carteira; recurso de outro → `404`.

---

## 11. Requisitos não-funcionais específicos

| Aspecto | Regra |
|---|---|
| Custo do recompute | O(ocorrências na janela), cap de 400 ocorrências |
| Fuso horário | Fixo `America/Sao_Paulo` no MVP (ver §9.1) |
| Privacidade do ranking | Respeita `OptOutLeaderboard`; nomes parciais para o aluno |
| Log | Sem PII (nome, e-mail) nos logs de gamificação |
| Idempotência | Recompute determinístico — reexecução não altera o resultado |
| Migration | Reversível; `Achievement` semeado com `Code` estável (sem GUID) |

---

## 12. Impacto no faseamento

Precisa de uma fatia de **backend** (entidades, migration, recompute, endpoints,
testes) antes de qualquer client consumir. Como o `roadmap.md` fecha a Fase 1
Backend com critério fixo, a gamificação entra como **tema de backlog com design
pronto** (este documento), a ser agendada após a Fase 2 e antes/junto da Fase 3
(app do aluno), que é onde ela aparece para o usuário final.

- **Backend de gamificação**: entidades + recompute + `GET /gamification/me` +
  ranking + testes.
- **Painel web (treinador)**: ver sequência/adesão do aluno no detalhe; placar da
  turma.
- **App do aluno (Fase 3)**: tela de progresso — sequência, dias, conquistas,
  ranking; animação ao desbloquear conquista.
- **Futuro**: job noturno + push ("sua sequência acaba hoje").

---

## Changelog

- **27 ago 2026**: criação. Escopo (streak + dias + conquistas + ranking) e regra
  de streak ("dia prescrito concluído") definidos com o dono do projeto. Modelo de
  dados, recompute sem job, catálogo de conquistas e superfície de API propostos;
  8 decisões finas deixadas em aberto na §9.
