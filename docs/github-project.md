# FitCoach — Organização do GitHub Project

Como o trabalho planejado nos outros documentos vira **Project, milestones,
labels e issues** no GitHub. Não descreve funcionalidade — é o elo entre o
planejamento e a execução. Complementa:

- [`roadmap.md`](./roadmap.md) — fases e itens pendentes (fonte das issues de
  Fase 1 e 2);
- [`requisitos.md`](./requisitos.md) — RF/RNF numerados (fonte dos épicos de
  backlog e da rastreabilidade dentro de cada issue).

**Levantado em 1 set 2026**, decidido em conversa com o dono do produto.
**Nada disso foi criado no GitHub ainda** — este documento é o plano; a
criação em si (Project, milestones, labels, issues) é um passo seguinte,
separado.

---

## 1. Modelo

**Um Project (v2) só**, não um por fase nem um por app — o mesmo motivo que
`requisitos.md` já é um documento único: é o "banco de dados" central do que
falta fazer, o Project só transforma isso num board.

### Campos

| Campo | Tipo | Valores | Fonte |
|---|---|---|---|
| **Status** | built-in (single select) | Backlog · Ready · In Progress · In Review · Done | fluxo de trabalho |
| **Módulo** | single select | AUTH·CONV·ALU·TRN·EXE·PLN·SES·GAM·AVA·WCH·FIN·RNF | `requisitos.md` §Notação |
| **Prioridade** | single select | Must·Should·Could·Won't | `requisitos.md` (MoSCoW) |
| **Requisito** | texto | ex. `RF-AVA-3` | rastreio de volta ao `requisitos.md` |

**Fase não é campo — é Milestone.** Dá barra de progresso nativa do GitHub
por fase ("7 de 9 fechadas"), que é o próprio critério de "pronto" que o
roadmap já define. Issue sem milestone = ainda não agendada (backlog).

### Granularidade — a regra muda conforme a distância no tempo

- **Fase 1 e 2** (concreto, começa já): **1 issue por item numerado do
  roadmap**, não por RF individual — é o tamanho de entrega que o
  `roadmap.md` já escopou ali. Cada issue referencia no corpo os RF-IDs que
  cobre.
- **Fase 3, Fase 4 e backlog (GAM, AVA, FIN)**: **1 issue-épico por módulo**,
  com checklist (tasklist) dos RF-IDs — sem issue individual por RF ainda.
  Só quebra em issues reais quando o módulo for puxado pra dentro de uma fase
  ativa. Evita issues murchando no board por meses, e ficando desatualizadas
  se uma decisão mudar o requisito antes de começar.
- **Decisões em aberto** (`requisitos.md` §13, 17 itens) **não viram issue**.
  Continuam vivendo só no doc; só entram no board quando decididas e
  destravarem um RF de verdade (a própria tabela de §13 já linka "requisito
  afetado", não precisa duplicar em outro lugar).

---

## 2. Milestones

`Fase 1 — Backend` · `Fase 2 — Painel Web` · `Fase 3 — Mobile` ·
`Fase 4 — Watch`. Backlog (GAM/AVA/FIN) e itens "fora de fase" (deploy) não
entram em milestone até serem puxados.

---

## 3. Issues de Fase 1 (9)

Mapeadas 1:1 nos itens pendentes de `roadmap.md` (seção "Fase 1").

| # | Issue | RF/RNF cobertos |
|---|---|---|
| 1 | `PUT /api/plans/{id}` — editar plano | RF-PLN-5 |
| 2 | Seed de exercícios globais | RF-EXE-5 |
| 3 | Convite de aluno (gera + autocadastro + marca usado) | RF-CONV-1 a 4 |
| 4 | CI/CD — `dotnet test` a cada PR | RNF-CONF-1 |
| 5 | CORS de produção | RNF-SEG-4 |
| 6 | Tokens em cookie `httpOnly` (lado API) | RNF-SEG-5 |
| 7 | Busca de aluno por CPF (campo + índice + endpoint + validação) | RF-ALU-2 a 5 |
| 8 | Endpoint de edição do perfil complementar do aluno | RF-ALU-6 |
| 9 | Configurar `FluentValidation` | RNF-VAL-1, VAL-2 |

---

## 4. Issues de Fase 2 (11)

| # | Issue | RF cobertos | Depende de |
|---|---|---|---|
| 1 | Tela de login | RF-TRN-5 | — |
| 2 | Lista de alunos | RF-TRN-3 | — |
| 3 | Detalhe do aluno | RF-TRN-4 | — |
| 4 | Edição de plano de treino (tela) | RF-PLN-6 | Fase 1 #1 |
| 5 | Migrar token pra cookie `httpOnly` (lado web) | RNF-SEG-5 | Fase 1 #6 |
| 6 | Tela de biblioteca de exercícios | RF-EXE-6 | — |
| 7 | Gerar/copiar link de convite (painel) | RF-CONV-5 | Fase 1 #3 |
| 8 | Tela pública de cadastro via convite | RF-CONV-6 | Fase 1 #3 |
| 9 | Busca por CPF + aviso de duplicidade (UI) | RF-ALU-8 | Fase 1 #7 |
| 10 | Tela `/students/new` | RF-ALU-7 | — |
| 11 | Tela de detalhe do plano (`/plans/{id}`) | RF-PLN-7 | Fase 1 #1 (mesma rota do fluxo de criação) |

---

## 5. Épicos de backlog (criados já, sem issues-filhas ainda)

- **Épico: Gamificação do aluno** (`modulo:gam`) — checklist `RF-GAM-1` a
  `14`, sem milestone. Design técnico completo em `gamificacao.md`.
- **Épico: Avaliação física e anamnese** (`modulo:ava`) — checklist
  `RF-AVA-1` a `10`, `13`, `14`, `15`; `RF-AVA-11` e `RF-AVA-12` entram como
  itens do checklist marcados "bloqueado por decisão §13 #15/#16" — não como
  trabalho pronto pra pegar.
- **Épico: Assinatura e financeiro** (`modulo:fin`) — checklist `RF-FIN-1` a
  `4`, prioridade `Won't`, só como registro (o próprio `requisitos.md` já
  marca esse módulo como "backlog — sem issue aberta").

---

## 6. Labels

`modulo:auth` `modulo:conv` `modulo:alu` `modulo:trn` `modulo:exe`
`modulo:pln` `modulo:ses` `modulo:gam` `modulo:ava` `modulo:wch` `modulo:fin`
`modulo:rnf` — mais `decisao-pendente`, aplicada em qualquer issue travada
numa das 17 decisões de `requisitos.md` §13.

---

## 7. Views recomendadas

1. **Por Fase** (board, agrupado por Milestone) — view principal de
   planejamento, respeita a ordem fixa Backend → Web → Mobile → Watch que o
   `roadmap.md` já define.
2. **Por Módulo** (tabela, agrupada por Módulo) — visão de "quanto falta" por
   épico (GAM/AVA/etc.), útil quando um deles for ativado.
3. **Pronto pra pegar** (tabela filtrada: Fase = fase corrente, Status =
   Ready, ordenado por Prioridade) — o que puxar a seguir na prática.

Sem view de timeline por data (layout "Roadmap" do GitHub Projects) — não há
datas-alvo definidas, só ordem de fases; forçar datas hoje criaria uma
promessa que não existe.

---

## Changelog

- **1 set 2026**: criação. Estrutura decidida com o dono do produto:
  granularidade por item do roadmap nas Fases 1/2, épicos com checklist pro
  backlog (GAM/AVA/FIN) criados desde já, Milestone = Fase. Resolvido de
  passagem o gap do `RF-PLN-7` sem item numerado no `roadmap.md` (agora item
  11 da Fase 2) antes de fechar a lista de issues aqui.
