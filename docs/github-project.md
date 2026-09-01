# FitCoach — Organização do GitHub Project

Como o trabalho planejado nos outros documentos vira **Project, milestones,
labels e issues** no GitHub. Não descreve funcionalidade — é o elo entre o
planejamento e a execução. Complementa:

- [`roadmap.md`](./roadmap.md) — fases e itens pendentes (fonte das issues de
  Fase 1 e 2);
- [`requisitos.md`](./requisitos.md) — RF/RNF numerados (fonte dos épicos de
  backlog e da rastreabilidade dentro de cada issue).

**Levantado em 1 set 2026**, decidido em conversa com o dono do produto.
**Criado de fato no GitHub em 1 set 2026** — Project, milestones, labels e as
23 issues iniciais já existem; ver os links em cada seção abaixo.

---

## 1. Modelo

**Um Project (v2) só**, não um por fase nem um por app — o mesmo motivo que
`requisitos.md` já é um documento único: é o "banco de dados" central do que
falta fazer, o Project só transforma isso num board.

**Project**: [FitCoach — Roadmap (#9)](https://github.com/users/denisiorodrigues/projects/9).

### Campos

| Campo | Tipo | Valores | Fonte |
|---|---|---|---|
| **Status** | built-in (single select) | Backlog · Ready · In Progress · In Review · Done | fluxo de trabalho |
| **Módulo** | single select | AUTH·CONV·ALU·TRN·EXE·PLN·SES·GAM·AVA·WCH·FIN·RNF | `requisitos.md` §Notação |
| **Prioridade** | single select | Must·Should·Could·Won't | `requisitos.md` (MoSCoW) |
| **Requisito** | texto | ex. `RF-AVA-3` | rastreio de volta ao `requisitos.md` |

`Status` é o campo nativo do GitHub — as opções padrão (Todo/In Progress/Done)
foram editadas pra bater com os 5 valores acima; nenhum item perdeu valor
nessa troca.

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

| Milestone | Issues |
|---|---|
| [Fase 1 — Backend](https://github.com/denisiorodrigues/fitcoach/milestone/1) | 9 (#16–24) |
| [Fase 2 — Painel Web](https://github.com/denisiorodrigues/fitcoach/milestone/2) | 11 (#25–35) |
| [Fase 3 — Mobile](https://github.com/denisiorodrigues/fitcoach/milestone/3) | vazio — entra quando a Fase 2 fechar |
| [Fase 4 — Watch](https://github.com/denisiorodrigues/fitcoach/milestone/4) | vazio — entra quando a Fase 3 fechar |

Backlog (GAM/AVA/FIN) e itens "fora de fase" (deploy) não entram em milestone
até serem puxados.

---

## 3. Issues de Fase 1 (9)

Mapeadas 1:1 nos itens pendentes de `roadmap.md` (seção "Fase 1"). Todas
marcadas `Status: Ready` — nenhuma depende de outra dentro da própria fase.

| # | Issue | RF/RNF cobertos |
|---|---|---|
| 1 | [#16 — `PUT /api/plans/{id}` — editar plano](https://github.com/denisiorodrigues/fitcoach/issues/16) | RF-PLN-5 |
| 2 | [#17 — Seed de exercícios globais](https://github.com/denisiorodrigues/fitcoach/issues/17) | RF-EXE-5 |
| 3 | [#18 — Convite de aluno (gera + autocadastro + marca usado)](https://github.com/denisiorodrigues/fitcoach/issues/18) | RF-CONV-1 a 4 |
| 4 | [#19 — CI/CD — `dotnet test` a cada PR](https://github.com/denisiorodrigues/fitcoach/issues/19) | RNF-CONF-1 |
| 5 | [#20 — CORS de produção](https://github.com/denisiorodrigues/fitcoach/issues/20) | RNF-SEG-4 |
| 6 | [#21 — Tokens em cookie `httpOnly` (lado API)](https://github.com/denisiorodrigues/fitcoach/issues/21) | RNF-SEG-5 |
| 7 | [#22 — Busca de aluno por CPF (campo + índice + endpoint + validação)](https://github.com/denisiorodrigues/fitcoach/issues/22) | RF-ALU-2 a 5 |
| 8 | [#23 — Endpoint de edição do perfil complementar do aluno](https://github.com/denisiorodrigues/fitcoach/issues/23) | RF-ALU-6 |
| 9 | [#24 — Configurar `FluentValidation`](https://github.com/denisiorodrigues/fitcoach/issues/24) | RNF-VAL-1, VAL-2 |

---

## 4. Issues de Fase 2 (11)

Todas marcadas `Status: Backlog` — a fase inteira depende da Fase 1 fechar, e
5 das 11 dependem de uma issue específica da Fase 1 (coluna "Depende de").

| # | Issue | RF cobertos | Depende de |
|---|---|---|---|
| 1 | [#25 — Tela de login](https://github.com/denisiorodrigues/fitcoach/issues/25) | RF-TRN-5 | — |
| 2 | [#26 — Lista de alunos](https://github.com/denisiorodrigues/fitcoach/issues/26) | RF-TRN-3 | — |
| 3 | [#27 — Detalhe do aluno](https://github.com/denisiorodrigues/fitcoach/issues/27) | RF-TRN-4 | — |
| 4 | [#28 — Edição de plano de treino (tela)](https://github.com/denisiorodrigues/fitcoach/issues/28) | RF-PLN-6 | #16 |
| 5 | [#29 — Migrar token pra cookie `httpOnly` (lado web)](https://github.com/denisiorodrigues/fitcoach/issues/29) | RNF-SEG-5 | #21 |
| 6 | [#30 — Tela de biblioteca de exercícios](https://github.com/denisiorodrigues/fitcoach/issues/30) | RF-EXE-6 | — |
| 7 | [#31 — Gerar/copiar link de convite (painel)](https://github.com/denisiorodrigues/fitcoach/issues/31) | RF-CONV-5 | #18 |
| 8 | [#32 — Tela pública de cadastro via convite](https://github.com/denisiorodrigues/fitcoach/issues/32) | RF-CONV-6 | #18 |
| 9 | [#33 — Busca por CPF + aviso de duplicidade (UI)](https://github.com/denisiorodrigues/fitcoach/issues/33) | RF-ALU-8 | #22 |
| 10 | [#34 — Tela `/students/new`](https://github.com/denisiorodrigues/fitcoach/issues/34) | RF-ALU-7 | — |
| 11 | [#35 — Tela de detalhe do plano (`/plans/{id}`)](https://github.com/denisiorodrigues/fitcoach/issues/35) | RF-PLN-7 | #16 (mesma rota do fluxo de criação) |

---

## 5. Épicos de backlog

Criados já, sem issues-filhas ainda, `Status: Backlog`, sem milestone.

- [**#36 — Épico: Gamificação do aluno**](https://github.com/denisiorodrigues/fitcoach/issues/36) (`modulo:gam`) — checklist `RF-GAM-1` a `14`. Design técnico completo em `gamificacao.md`.
- [**#37 — Épico: Avaliação física e anamnese**](https://github.com/denisiorodrigues/fitcoach/issues/37) (`modulo:ava`) — checklist `RF-AVA-1` a `10`, `13`, `14`, `15`; `RF-AVA-11` e `RF-AVA-12` marcados no checklist como "bloqueado por decisão §13 #15/#16" — não como trabalho pronto pra pegar.
- [**#38 — Épico: Assinatura e financeiro**](https://github.com/denisiorodrigues/fitcoach/issues/38) (`modulo:fin`) — checklist `RF-FIN-1` a `4`, prioridade `Won't`, só como registro (o próprio `requisitos.md` já marca esse módulo como "backlog — sem issue aberta").

---

## 6. Labels

Criadas: `modulo:auth` `modulo:conv` `modulo:alu` `modulo:trn` `modulo:exe`
`modulo:pln` `modulo:ses` `modulo:gam` `modulo:ava` `modulo:wch` `modulo:fin`
`modulo:rnf` — mais `decisao-pendente`, pra aplicar em qualquer issue travada
numa das 17 decisões de `requisitos.md` §13 (nenhuma issue usa essa label
ainda — nenhuma decisão foi tomada desde a criação).

---

## 7. Views — pendente, criação manual

1. **Por Fase** (board, agrupado por Milestone) — view principal de
   planejamento, respeita a ordem fixa Backend → Web → Mobile → Watch que o
   `roadmap.md` já define.
2. **Por Módulo** (tabela, agrupada por Módulo) — visão de "quanto falta" por
   épico (GAM/AVA/etc.), útil quando um deles for ativado.
3. **Pronto pra pegar** (tabela filtrada: Fase = fase corrente, Status =
   Ready, ordenado por Prioridade) — o que puxar a seguir na prática. Hoje
   isso já filtra certo: as 9 issues de Fase 1 estão em `Ready`, o resto em
   `Backlog`.

GitHub não expõe criação de view por API/CLI — as 3 de cima ainda não foram
criadas, é um passo manual rápido direto no Project (botão "+ New view").

Sem view de timeline por data (layout "Roadmap" do GitHub Projects) — não há
datas-alvo definidas, só ordem de fases; forçar datas hoje criaria uma
promessa que não existe.

---

## Changelog

- **1 set 2026**: Project, milestones, labels e as 23 issues iniciais
  (9 de Fase 1 + 11 de Fase 2 + 3 épicos de backlog) criados de fato no
  GitHub — links atualizados em cada seção. O campo `Status` nativo ganhou os
  5 valores do plano (Backlog/Ready/In Progress/In Review/Done, editados a
  partir do padrão Todo/In Progress/Done, sem perder valor dos itens já
  criados); as 9 issues de Fase 1 (sem dependência entre si) marcadas
  `Ready`, o resto em `Backlog`. As 3 views recomendadas (§7) ainda não foram
  criadas — GitHub não expõe isso via API/CLI, fica como passo manual.
- **1 set 2026**: criação. Estrutura decidida com o dono do produto:
  granularidade por item do roadmap nas Fases 1/2, épicos com checklist pro
  backlog (GAM/AVA/FIN) criados desde já, Milestone = Fase. Resolvido de
  passagem o gap do `RF-PLN-7` sem item numerado no `roadmap.md` (agora item
  11 da Fase 2) antes de fechar a lista de issues aqui.
