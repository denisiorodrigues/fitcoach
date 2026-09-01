# FitCoach — Documento de Requisitos

Requisitos funcionais (RF) e não-funcionais (RNF) do FitCoach, numerados e
rastreáveis. Complementa:

- [`plano-de-negocio.md`](./plano-de-negocio.md) — visão de produto, mercado, receita;
- [`roadmap.md`](./roadmap.md) — em que fase cada pendência é implementada;
- [`regras-de-negocio.md`](./regras-de-negocio.md) — detalhe de cada campo e regra;
- [`gamificacao.md`](./gamificacao.md) — design da gamificação do aluno (módulo RF-GAM);
- [`architecture.md`](./architecture.md) — decisões técnicas;
- [`manual-do-usuario.md`](./manual-do-usuario.md) — como cada tela funciona hoje.

**Escrito em 27 ago 2026**, derivado dos documentos acima (levantados direto do
código em 20–25 ago 2026). Onde um requisito ainda não existe no código, o status
diz isso — não descreve como se já existisse.

---

## Notação

- **ID**: `RF-<módulo>-<n>` (funcional), `RNF-<categoria>-<n>` (não-funcional).
  - Módulos: **AUTH** (autenticação), **CONV** (convite/autocadastro),
    **ALU** (aluno/perfil/CPF), **TRN** (painel do treinador),
    **EXE** (exercícios), **PLN** (planos de treino), **SES** (sessões/execução),
    **GAM** (gamificação do aluno), **AVA** (avaliação física do aluno),
    **WCH** (relógio), **FIN** (assinatura/financeiro).
  - Categorias RNF: **SEG** (segurança), **DES** (desempenho/escala),
    **CONF** (confiabilidade/entrega), **USA** (usabilidade),
    **ARQ** (portabilidade/arquitetura), **VAL** (validação de dados),
    **LEG** (conformidade legal).
- **Prioridade (MoSCoW)**: `Must` · `Should` · `Could` · `Won't` (agora — está no
  backlog).
- **Status**: ✅ implementado · 🟡 parcial (ex.: API pronta, sem tela) · ⬜ pendente.
- **Fase**: fase do [`roadmap.md`](./roadmap.md) que entrega o requisito
  (`—` = já entregue, fora de fase).
- **Regra**: seção do [`regras-de-negocio.md`](./regras-de-negocio.md) (`§N`) ou
  outro doc que detalha a regra.

---

## 1. RF — AUTH (autenticação e cadastro)

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-AUTH-1 | Autocadastro público de treinador (`POST /api/auth/register/trainer`): name, email, password obrigatórios; specialty e crefNumber opcionais | Must | ✅ | — | §2 |
| RF-AUTH-2 | Cadastro direto de aluno pelo treinador autenticado (`POST /api/auth/register/student`, exige JWT de Trainer); vínculo vem do token | Must | ✅ | — | §3a |
| RF-AUTH-3 | Login trainer/student (`POST /api/auth/login`); `401` se e-mail inexistente, senha errada ou `isActive = false` | Must | ✅ | — | §5 |
| RF-AUTH-4 | Emitir JWT válido por 7 dias + refresh token | Must | ✅ | — | §5 |
| RF-AUTH-5 | Papéis fixos (Trainer/Student), definidos no cadastro e gravados no token (claims `role` + `profileId`); usuário nunca muda de papel | Must | ✅ | — | §1 |
| RF-AUTH-6 | E-mail único em todo o sistema (trainer e student no mesmo espaço), normalizado para minúsculas | Must | ✅ | — | §2, §3 |
| RF-AUTH-7 | Validar formato de e-mail e força mínima de senha no backend | Should | ⬜ | Fase 1 (item 9) | §10, §11 |
| RF-AUTH-8 | Fluxo de senha temporária + "primeiro acesso, troque a senha" no cadastro direto de aluno | Could | ⬜ | decisão aberta | §11 |

---

## 2. RF — CONV (convite e autocadastro do aluno)

Fluxo planejado, ainda não implementado. Convive com o cadastro direto (RF-AUTH-2),
não o substitui.

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-CONV-1 | Treinador gera convite único por aluno: link/código de uso único, com validade (sugestão 7 dias) | Must | ⬜ | Fase 1 (item 3) | roadmap Fase 1.3 |
| RF-CONV-2 | Endpoint público de autocadastro do aluno via código de convite (sem JWT) | Must | ⬜ | Fase 1 (item 3) | roadmap Fase 1.3 |
| RF-CONV-3 | Convite marcado como usado após o cadastro; vínculo com o treinador vem do convite | Must | ⬜ | Fase 1 (item 3) | roadmap Fase 1.3 |
| RF-CONV-4 | Aluno informa os próprios dados no autocadastro: nome, e-mail, senha, CPF | Must | ⬜ | Fase 1 (item 3) | §3b |
| RF-CONV-5 | Painel do treinador: ação para gerar/copiar o link de convite (provável na lista de alunos) | Should | ⬜ | Fase 2 (item 7) | roadmap Fase 2.7 |
| RF-CONV-6 | Tela pública de cadastro via convite (web, sem login); ao final redireciona para `/login` (desktop) ou link de download do app (mobile, com fallback para `/login`) | Must | ⬜ | Fase 2 (item 8) | roadmap Fase 2.8 |

---

## 3. RF — ALU (perfil complementar do aluno e CPF)

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-ALU-1 | `StudentProfile` com campos opcionais: birthDate, weightKg, heightCm, goal (≤300), healthNotes (≤500) | Must | ✅ | — | §4 |
| RF-ALU-2 | Campo `cpf` obrigatório em `StudentProfile`, único por treinador (índice composto `TrainerId`+`CPF`); mesmo CPF pode existir sob treinadores diferentes | Must | ⬜ | Fase 1 (item 7) | §4 |
| RF-ALU-3 | Validar formato de CPF (11 dígitos + dígitos verificadores) | Must | ⬜ | Fase 1 (item 7) | roadmap Fase 1.7 |
| RF-ALU-4 | `GET /api/students/search?cpf=...`, restrito ao treinador autenticado, para checar duplicidade antes do cadastro | Must | ⬜ | Fase 1 (item 7) | roadmap Fase 1.7 |
| RF-ALU-5 | Validação de unicidade de CPF no momento de salvar (rede de segurança), nos dois fluxos de criação de aluno | Must | ⬜ | Fase 1 (item 7) | roadmap Fase 1.7 |
| RF-ALU-6 | Endpoint de edição do perfil complementar do aluno (DTO `UpdateStudentProfileRequest` já existe, nenhum controller o usa) | Must | ⬜ | Fase 1 (item 8) | §4 |
| RF-ALU-7 | Tela `/students/new` (cadastro direto de aluno pelo treinador: nome, e-mail, senha, CPF) — hoje o dashboard já linka para ela, mas não existe | Must | ⬜ | Fase 2 (item 10) | roadmap Fase 2.10 |
| RF-ALU-8 | Busca por CPF na UI com aviso de duplicidade **só** quando o CPF já existe na carteira do mesmo treinador | Should | ⬜ | Fase 2 (item 9) | roadmap Fase 2.9 |

---

## 4. RF — TRN (painel do treinador)

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-TRN-1 | Dashboard com 4 contadores: total de alunos, ativos na semana (treinaram nos últimos 7 dias), treinos prescritos (total de planos), taxa de adesão (% de ativos) | Must | ✅ | — | §8, manual §1 |
| RF-TRN-2 | Dashboard: lista "Atividade dos Alunos" — até 8 alunos ordenados pelo treino mais recente, com nome, tempo desde o último treino, etiqueta Ativo/Inativo e nº de treinos no mês | Must | ✅ | — | manual §1 |
| RF-TRN-3 | Listar alunos do treinador autenticado (`GET /api/students`) | Must | 🟡 (API ✅, tela ⬜) | Fase 2 (item 2) | §9 |
| RF-TRN-4 | Detalhe do aluno + histórico de atividade (`GET /api/students/{id}` e `/activity` — últimas 10 sessões + contadores do mês) | Must | 🟡 (API ✅, tela ⬜) | Fase 2 (item 3) | §8, §9 |
| RF-TRN-5 | Tela de login do painel web (hoje `api.ts` já redireciona para `/login` no `401`, mas a rota não existe) | Must | ⬜ | Fase 2 (item 1) | manual §3 |
| RF-TRN-6 | Perfil do treinador: specialty (texto livre ≤200), crefNumber (registro no CREF) | Should | 🟡 (sem validação de formato) | — | §2 |
| RF-TRN-7 | Definir se `crefNumber` é obrigatório | Could | ⬜ | decisão aberta | §11 |

---

## 5. RF — EXE (exercícios)

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-EXE-1 | Listar exercícios com filtro por grupo muscular e equipamento (`GET /api/exercises`) | Must | ✅ | — | §6 |
| RF-EXE-2 | Criar exercício próprio (`POST /api/exercises`, só Trainer) | Must | ✅ | — | §6 |
| RF-EXE-3 | Visibilidade: exercício aparece para o treinador que o criou (`isGlobal=false`) ou se for global (`isGlobal=true`, visível para todos) | Must | ✅ | — | §6 |
| RF-EXE-4 | `muscleGroup` ∈ 10 valores fixos; `equipment` ∈ 8 valores fixos; valor inválido → `400` | Must | ✅ | — | §6 |
| RF-EXE-5 | Biblioteca padrão de exercícios globais efetivamente semeada no banco (`SeedDefaultExercises` monta a lista mas nunca chama `.HasData(...)`); exige GUIDs fixos + migration nova | Must | ⬜ | Fase 1 (item 2) | §6 |
| RF-EXE-6 | Tela própria de biblioteca de exercícios (listar com filtro + criar exercício), separada do seletor embutido na criação de plano | Should | ⬜ | Fase 2 (item 6) | roadmap Fase 2.6 |
| RF-EXE-7 | Editar / excluir exercício | Won't | ⬜ | backlog | roadmap Fase 2.6 (nota) |

---

## 6. RF — PLN (planos de treino)

Estrutura: Plano → Dias (`WorkoutDay`) → Exercícios do dia (`PlanExercise`).

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-PLN-1 | Criar plano completo (plano + dias + exercícios do dia) em uma única chamada (`POST /api/plans`) | Must | ✅ | — | §7 |
| RF-PLN-2 | Listar planos criados pelo treinador (`GET /api/plans`) | Must | ✅ | — | §7 |
| RF-PLN-3 | Detalhe completo do plano (`GET /api/plans/{id}`) — acessível ao Trainer dono ou ao Student dono | Must | ✅ | — | §7 |
| RF-PLN-4 | `studentId` do plano precisa pertencer ao treinador autenticado, senão `400` | Must | ✅ | — | §7 |
| RF-PLN-5 | Editar plano depois de criado (`PUT /api/plans/{id}`; DTO `UpdateWorkoutPlanRequest` já existe; o client web já chama esse endpoint) | Must | ⬜ | Fase 1 (item 1) | §7 |
| RF-PLN-6 | Tela de edição de plano no painel web | Must | ⬜ | Fase 2 (item 4) | manual §8 |
| RF-PLN-7 | Tela de detalhe do plano (`/plans/{id}`) — hoje salvar um plano leva a uma página inexistente | Should | ⬜ | Fase 2 | manual §2 |
| RF-PLN-8 | Restrições de tamanho de campo (name ≤150, description ≤500, label do dia ≤60, notes ≤200, reps ≤20, coachNotes ≤300) | Should | 🟡 (parcial, via `MaxLength` do EF) | Fase 1 (item 9) | §7, §10 |
| RF-PLN-9 | Definir se um plano com lista de `days` vazia é permitido ou vira `400` | Could | ⬜ | decisão aberta | §7, §11 |

---

## 7. RF — SES (execução de treino — aluno)

Só o papel Student acessa.

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-SES-1 | Iniciar sessão (`POST /api/sessions/start`, `workoutDayId`); grava `startedAt` automaticamente | Must | ✅ | — | §8 |
| RF-SES-2 | Registrar série (`POST /api/sessions/{id}/sets`: planExerciseId, setNumber, repsDone, weightKg); só na sessão do próprio aluno (senão `404`) | Must | ✅ | — | §8 |
| RF-SES-3 | Finalizar sessão (`POST /api/sessions/{id}/finish`: avgHeartRate, caloriesBurned, studentNotes — todos opcionais); calcula `durationSeconds` | Must | ✅ | — | §8 |
| RF-SES-4 | Detalhe de sessão com séries (`GET /api/sessions/{id}`) | Must | ✅ | — | §8 |
| RF-SES-5 | Dashboard do aluno (`GET /api/dashboard`): treino do dia (dia da semana atual × plano ativo), próximo treino, recordes pessoais (maior peso por exercício), contadores de sessões (mês / total) | Must | ✅ | — | §8 |
| RF-SES-6 | App mobile do aluno (React Native): login com conta já criada | Must | ⬜ | Fase 3 | roadmap Fase 3.1 |
| RF-SES-7 | App: dashboard do aluno (RF-SES-5) e executar treino (RF-SES-1..3) | Must | ⬜ | Fase 3 | roadmap Fase 3.2–3.3 |
| RF-SES-8 | App: histórico de sessões / detalhe de sessão | Should | ⬜ | Fase 3 | roadmap Fase 3.4 |
| RF-SES-9 | App: perfil do aluno (leitura dos dados cadastrados pelo treinador) | Should | ⬜ | Fase 3 | roadmap Fase 3.5 |

---

## 8. RF — GAM (gamificação do aluno)

Épico novo. Design técnico completo em [`gamificacao.md`](./gamificacao.md);
motivação (retenção) em [`plano-de-negocio.md`](./plano-de-negocio.md) §3.4.
Precisa de uma fatia de backend própria (backlog), aparece para o usuário na
Fase 3 (app do aluno). Prioridades abaixo são **dentro do épico** — o épico
inteiro é posterior à Fase 2.

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-GAM-1 | Sequência (streak) de treinos prescritos concluídos em dias seguidos: valor atual + melhor sequência histórica | Must | ⬜ | backlog / Fase 3 | gamificacao.md §4 |
| RF-GAM-2 | Ocorrência prescrita = data cujo dia da semana casa com um `WorkoutDay` do plano ativo naquela data | Must | ⬜ | backlog | gamificacao.md §4.1 |
| RF-GAM-3 | Ocorrência prescrita vencida e não concluída zera a sequência; dia de descanso (sem prescrição) não penaliza | Must | ⬜ | backlog | gamificacao.md §4.2 |
| RF-GAM-4 | Sessão conta pela data de `startedAt` (fuso do aluno), com tolerância de 24h para o `finishedAt` | Should | ⬜ | backlog | gamificacao.md §4.1 |
| RF-GAM-5 | Sem plano ativo numa data, a sequência **pausa** (não quebra) | Should | ⬜ | decisão aberta | gamificacao.md §9.2 |
| RF-GAM-6 | Contador de dias treinados (mês + acumulado) e última data treinada | Should | 🟡 (contadores mês/total já no dashboard) | backlog | gamificacao.md §2 |
| RF-GAM-7 | Catálogo de conquistas semeado + desbloqueio no recompute; conquista nunca é revogada | Should | ⬜ | backlog | gamificacao.md §6 |
| RF-GAM-8 | Pontos: 10 por ocorrência concluída + pontos das conquistas; sem penalidade por perder dia | Could | ⬜ | backlog | gamificacao.md §7 |
| RF-GAM-9 | Ranking entre alunos do mesmo treinador (`GET /api/gamification/leaderboard`) | Could | ⬜ | backlog | gamificacao.md §8 |
| RF-GAM-10 | Privacidade do ranking: nome parcial para o aluno; flag `OptOutLeaderboard`; treinador vê o placar completo | Could | ⬜ | decisão aberta | gamificacao.md §8 |
| RF-GAM-11 | `GET /api/gamification/me` + bloco `gamification` resumido no dashboard do aluno | Should | ⬜ | backlog / Fase 3 | gamificacao.md §10 |
| RF-GAM-12 | Treinador vê a gamificação de um aluno (`GET /api/students/{id}/gamification`) — estende RF-TRN-4 | Should | ⬜ | backlog | gamificacao.md §10 |
| RF-GAM-13 | Recompute determinístico e idempotente, disparado ao finalizar sessão e na leitura; sem job agendado no MVP; cap de 400 ocorrências | Should | ⬜ | backlog | gamificacao.md §5 |
| RF-GAM-14 | App do aluno exibe sequência, dias treinados, conquistas e ranking | Should | ⬜ | Fase 3 | gamificacao.md §12 |

---

## 9. RF — AVA (avaliação física e anamnese do aluno)

Épico novo, escopado com o dono do produto em 31 ago 2026 — resolve a decisão #14
que estava aberta em §13 ("Avaliação física / anamnese... entra no produto ou fica
fora de escopo?"): **entra**. Motivação: table stake para consultoria online
([`plano-de-negocio.md`](./plano-de-negocio.md) §5.5). Cobre anamnese + três
frentes de medida corporal (bioimpedância, dobras cutâneas/adipômetro,
circunferências/fita métrica) + histórico de avaliações do aluno + fotos/vídeos
de acompanhamento enviados pelo treinador + feedback do aluno sobre a avaliação
recebida. A anamnese estruturada aqui **substitui** o uso
do campo livre `healthNotes` de RF-ALU-1 como anamnese informal — `healthNotes`
continua existindo para observações rápidas do treinador, sem sobreposição de
função.

Como GAM (§8), precisa de fatia própria de backend (entidade, endpoints, testes) +
tela de registro no painel web do treinador (quem aplica a avaliação), a agendar
no backlog; a visualização do histórico e o feedback do aluno aparecem para ele na
**Fase 3** (app do aluno), quando esse app existir.

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-AVA-1 | Entidade `PhysicalEvaluation`: uma avaliação = 1 registro datado (`EvaluatedAt`), vinculado a `StudentId` + `TrainerId` (quem aplicou); inclui peso (kg) e altura (cm) capturados nessa data — sem isso não dá pra montar o histórico de evolução do RF-AVA-7; ao salvar, atualiza também o snapshot atual em `StudentProfile.WeightKg`/`HeightCm` (RF-ALU-1); um aluno pode ter várias avaliações ao longo do tempo | Must | ⬜ | backlog | módulo novo |
| RF-AVA-2 | Anamnese estruturada por avaliação: histórico de saúde, lesões/cirurgias prévias, doenças pré-existentes, medicamentos em uso, nível de atividade física atual, restrições | Must | ⬜ | backlog | módulo novo |
| RF-AVA-3 | Bioimpedância: % de gordura corporal, massa gorda (kg), massa magra (kg), massa muscular (kg), massa óssea (kg), água corporal total (%), água intracelular e extracelular — ICW/ECW (%), massa de proteína (kg), taxa metabólica basal (kcal), idade metabólica, gordura visceral (índice/nível), análise segmentar (gordura e massa magra por braço/tronco/perna) e pontuação/score do aparelho (campo livre opcional, já que é proprietário de cada marca); preenchido manualmente pelo treinador a partir da leitura do aparelho — é a forma de entrada do MVP; captura facilitada é backlog futuro (RF-AVA-13) | Must | ⬜ | backlog | módulo novo |
| RF-AVA-4 | Dobras cutâneas (adipômetro), em mm, por ponto de medição: tricipital, subescapular, axilar média, suprailíaca, abdominal, coxa, peitoral (protocolo Pollock 7 dobras) | Must | ⬜ | backlog | módulo novo |
| RF-AVA-5 | Circunferências (fita métrica), em cm: pescoço, ombro, tórax, cintura, abdômen, quadril, braço (dir./esq., relaxado e contraído), antebraço (dir./esq.), coxa (dir./esq.), panturrilha (dir./esq.) | Must | ⬜ | backlog | módulo novo |
| RF-AVA-6 | `POST/GET /api/students/{id}/evaluations`: treinador registra e lista as avaliações do próprio aluno; isolamento por dono como os demais módulos (RNF-SEG-2) | Must | ⬜ | backlog | módulo novo |
| RF-AVA-7 | Tela no painel web do treinador: formulário de registro (anamnese + as 3 frentes de medida), histórico/evolução do aluno (comparação entre avaliações ao longo do tempo, incluindo peso/altura do RF-AVA-1) e exibição do feedback que o aluno deixou em cada avaliação (RF-AVA-9) | Must | ⬜ | backlog | módulo novo |
| RF-AVA-8 | Aluno visualiza no app o histórico das próprias avaliações (medidas + evolução) | Should | ⬜ | Fase 3 | módulo novo |
| RF-AVA-9 | Aluno registra feedback em texto sobre uma avaliação recebida (`POST /api/evaluations/{id}/feedback`), visível ao treinador | Must | ⬜ | Fase 3 | módulo novo |
| RF-AVA-10 | Fotos e vídeos de acompanhamento/orientação por avaliação, enviados pelo treinador (fotos já citadas no backlog original do roadmap; vídeo é pedido novo do dono do produto, 31 ago 2026). Armazenamento: VPS Hostinger no início do projeto (restrição de orçamento — decisão de 31 ago 2026), migração para nuvem de mercado (AWS/Azure/GCP) planejada depois. Exige consentimento explícito do aluno antes de armazenar (RNF-LEG-4); limite de tamanho de arquivo e duração de vídeo é decisão técnica em aberto (§13) | Should | ⬜ | backlog | roadmap "Fora de fase" |
| RF-AVA-11 | Protocolo de dobras cutâneas: 3 ou 7 pontos; sistema calcula % de gordura pela fórmula do protocolo automaticamente, ou só arquiva os valores brutos digitados pelo treinador | Could | ⬜ | decisão aberta | módulo novo |
| RF-AVA-12 | Estrutura do feedback do aluno (RF-AVA-9): só texto livre, ou também nota/rating estruturado? Editável depois de enviado? | Could | ⬜ | decisão aberta | módulo novo |
| RF-AVA-13 | Captura facilitada de bioimpedância, pra substituir a digitação manual do RF-AVA-3: (a) foto da tela do aparelho + OCR extraindo os valores automaticamente, e/ou (b) integração via API com o fabricante da balança, se algum disponibilizar uma. **Backlog futuro** — pedido do dono do produto em 31 ago 2026, sem prazo definido | Won't | ⬜ | backlog futuro | módulo novo |
| RF-AVA-14 | Editar uma avaliação depois de criada (`PUT /api/evaluations/{id}`), mesmo padrão do `RF-PLN-5`; exclusão fica fora de escopo por ora (mesmo tratamento do `RF-EXE-7`) — apagar uma avaliação quebraria o histórico de evolução do RF-AVA-7 | Should | ⬜ | backlog | módulo novo |

---

## 10. RF — WCH (relógio: watchOS + Wear OS)

Nada implementado. Módulo de lógica compartilhada em Kotlin Multiplatform + UI
nativa por plataforma (`architecture.md §5`).

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-WCH-1 | Registrar a sessão de treino (séries, FC) localmente no relógio, mesmo sem conexão | Could | ⬜ | Fase 4 | architecture §5.4 |
| RF-WCH-2 | Sincronizar os dados quando a conexão voltar (direto com a API ou via phone — decisão aberta) | Could | ⬜ | Fase 4 | architecture §5.4 |
| RF-WCH-3 | UI nativa watchOS (SwiftUI): iniciar/acompanhar treino, timer, FC | Could | ⬜ | Fase 4 | roadmap Fase 4.2 |
| RF-WCH-4 | UI nativa Wear OS (Jetpack Compose): idem | Could | ⬜ | Fase 4 | roadmap Fase 4.3 |
| RF-WCH-5 | Integração com HealthKit (watchOS) / Health Services API (Wear OS) para frequência cardíaca | Could | ⬜ | Fase 4 | roadmap Fase 4.4 |

---

## 11. RF — FIN (assinatura e financeiro)

Backlog — sem issue aberta. Detalhado no [`plano-de-negocio.md`](./plano-de-negocio.md) §6–§7.

| ID | Requisito | Prio | Status | Fase | Regra |
|---|---|---|---|---|---|
| RF-FIN-1 | Assinatura SaaS do treinador (mensal/anual) | Won't | ⬜ | backlog | plano-de-negocio §6 |
| RF-FIN-2 | Faixas de plano por número de alunos ativos (Free / Pro / Academia) | Won't | ⬜ | backlog | plano-de-negocio §7 |
| RF-FIN-3 | Pagamento recorrente via Stripe / Pagar.me | Won't | ⬜ | backlog | roadmap backlog |
| RF-FIN-4 | Multi-academia: vários professores por estabelecimento, com visão consolidada | Won't | ⬜ | backlog | roadmap backlog |

---

## 12. RNF — Requisitos não-funcionais

### 12.1 Segurança (SEG)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-SEG-1 | Senhas guardadas como hash BCrypt | Must | ✅ | — | §2, architecture §2 |
| RNF-SEG-2 | Isolamento por dono em todos os endpoints: sem token → `401`; papel errado → `403`; recurso de outro treinador/aluno → `404` (nunca revela que o recurso existe — IDOR coberto) | Must | ✅ | — | §9 |
| RNF-SEG-3 | `Jwt__Key` com no mínimo 32 caracteres, gerado aleatoriamente | Must | ✅ | — | README |
| RNF-SEG-4 | CORS de produção: hoje libera só `localhost:3000` e o placeholder `fitcoach.yourdomain.com` — trocar pelo domínio real antes de expor o painel | Must | ⬜ | Fase 1 (item 5) | architecture §6 |
| RNF-SEG-5 | Tokens em cookie `httpOnly` + `Secure` + `SameSite` (hoje a API devolve token + refreshToken no corpo e o web guarda em `localStorage`) — mudança de contrato API↔client, os dois lados mudam juntos | Must | ⬜ | Fase 1 (item 6) / Fase 2 (item 5) | architecture §6 |
| RNF-SEG-6 | Dados sensíveis de saúde/atividade (`healthNotes`, FC, calorias, anamnese, medidas e fotos/vídeos de avaliação física — RF-AVA): cache local com expiração, nunca em log, tráfego sempre em TLS | Must | ⬜ (relevante ao construir mobile) | Fase 3 | architecture §6 |

### 12.2 Desempenho e escala (DES)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-DES-1 | Dashboard do treinador atualiza sozinho a cada 1 minuto | Should | ✅ | — | manual §1 |
| RNF-DES-2 | Persistência em PostgreSQL 16 | Must | ✅ | — | architecture §2 |
| RNF-DES-3 | Criação de plano completo (dias + exercícios) em uma única chamada, evitando N requisições do client | Should | ✅ | — | §7 |
| RNF-DES-4 | Metas de tempo de resposta por endpoint | Should | ⬜ | a definir | — |
| RNF-DES-5 | Recompute de gamificação com custo O(ocorrências na janela) e cap de 400 ocorrências; sem job agendado no MVP | Should | ⬜ | backlog | gamificacao.md §5, §11 |

### 12.3 Confiabilidade e entrega (CONF)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-CONF-1 | CI rodando `dotnet test` a cada PR (não há nenhum workflow do projeto em `.github/workflows` hoje) | Must | ⬜ | Fase 1 (item 4) | roadmap Fase 1.4 |
| RNF-CONF-2 | Suíte de testes automatizados mantida e crescente (hoje: 61 testes — 30 unit + 31 integração) | Must | ✅ | — | roadmap Fase 1 |
| RNF-CONF-3 | Migrations aplicadas automaticamente na subida da API (testes de integração usam banco em memória) | Must | ✅ | — | architecture §2 |

### 12.4 Usabilidade (USA)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-USA-1 | Painel web responsivo (Tailwind CSS) | Should | 🟡 | Fase 2 | architecture §2 |
| RNF-USA-2 | Registro de série funciona offline no relógio; o relógio opera 100% a partir do cache local durante o treino | Could | ⬜ | Fase 4 | architecture §5.4 |
| RNF-USA-3 | Auto-login após o cadastro: trainer e student recebem JWT direto, sem precisar logar de novo | Should | ✅ | — | §2, §3 |

### 12.5 Portabilidade e arquitetura (ARQ)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-ARQ-1 | Repositório em monorepo (`apps/` + `packages/`) | Must | ✅ | — | architecture §5.3 |
| RNF-ARQ-2 | Stack fixada por camada: .NET 10 (API), Next.js 14 (web), React Native (phone), KMP + SwiftUI/Compose (watch) | Must | 🟡 (API e web existem) | Fases 3–4 | architecture §5.2 |
| RNF-ARQ-3 | Lógica de negócio de treino/prescrição centralizada na API; clientes majoritariamente consumidores (exceto tracking offline no watch) | Must | ✅ | — | architecture §5.1 |
| RNF-ARQ-4 | Empacotamento e subida do ambiente via Docker Compose (`postgres`, `api`, `web`) | Should | ✅ | — | architecture §2 |

### 12.6 Validação de dados (VAL)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-VAL-1 | Configurar `FluentValidation` (dependência instalada, nunca registrada; nenhum validator existe): criar validators e registrar no `Program.cs` | Must | ⬜ | Fase 1 (item 9) | §10 |
| RNF-VAL-2 | Cobrir as validações hoje ausentes: formato de e-mail, força de senha, tamanho mínimo de nome, plano sem nenhum dia (hoje só há `MaxLength` do EF + rejeição de `null`, não de string vazia) | Must | ⬜ | Fase 1 (item 9) | §10 |

### 12.7 Conformidade legal (LEG)

| ID | Requisito | Prio | Status | Fase | Fonte |
|---|---|---|---|---|---|
| RNF-LEG-1 | Tratamento de dados de saúde conforme LGPD: base legal, política de privacidade, minimização e retenção — antes de captar aluno real | Must | ⬜ | antes da Fase 3 | architecture §6, plano-de-negocio §10 |
| RNF-LEG-2 | Definir a exigência de registro no CREF para o treinador (credibilidade + risco de habilitar leigo a prescrever treino) | Should | ⬜ | decisão aberta | §11 |
| RNF-LEG-3 | Ranking de gamificação respeita `OptOutLeaderboard` e expõe só nome parcial para os alunos; nenhum dado de saúde no placar | Should | ⬜ | backlog | gamificacao.md §8, §11 |
| RNF-LEG-4 | Consentimento explícito do aluno antes de armazenar fotos/vídeos do corpo (RF-AVA-10) — dado de imagem sensível, além da anamnese/medidas já cobertas por RNF-SEG-6 | Must | ⬜ | backlog | módulo novo |

---

## 13. Decisões de escopo em aberto

Viram requisitos concretos assim que a decisão for tomada.

| # | Questão | Onde está | Requisito afetado |
|---|---|---|---|
| 1 | CREF obrigatório no cadastro do treinador? | §11 (regras-de-negocio) | RF-TRN-7, RNF-LEG-2 |
| 2 | Como o aluno recebe a senha no cadastro direto? Fora do sistema, ou senha temporária + primeiro acesso? | §11 (regras-de-negocio) | RF-AUTH-8 |
| 3 | Regras de senha e validação de formato de e-mail no backend | §11 (regras-de-negocio) | RF-AUTH-7, RNF-VAL-2 |
| 4 | Plano com `days` vazio: permitido ou `400`? | §7, §11 (regras-de-negocio) | RF-PLN-9 |
| 5 | Prazo de validade do convite de aluno (sugestão: 7 dias) | roadmap Fase 1.3 | RF-CONV-1 |
| 6 | O relógio sincroniza direto com a API ou sempre via phone como ponte Bluetooth? | roadmap Fase 4, architecture §5.4 | RF-WCH-2 |
| 7 | Persistência local do módulo `watch-shared`: arquivo simples ou SQLite via KMP? | roadmap Fase 4, architecture §7 | RF-WCH-1 |
| 8 | Deploy/infra de produção é critério de "pronto" da Fase 1/2 ou fase própria de publicação? | roadmap "Fora de fase" | RNF-SEG-4, RNF-CONF-1 |
| 9 | Gamificação: fuso por aluno (campo `TimeZone`) ou fixo `America/Sao_Paulo`? | gamificacao.md §9 | RF-GAM-2, RF-GAM-4 |
| 10 | Gamificação: sem plano ativo, a sequência pausa ou quebra após N dias? | gamificacao.md §9 | RF-GAM-5 |
| 11 | Ranking: `OptOutLeaderboard` ligado ou desligado por padrão; métrica de ordenação (sequência / pontos / dias no mês); aluno fora do ranking vê a própria posição? | gamificacao.md §9 | RF-GAM-9, RF-GAM-10 |
| 12 | Gamificação: "semana perfeita" = seg–dom? "Congelar sequência" (streak freeze) entra algum dia? | gamificacao.md §9 | RF-GAM-7 |
| 13 | **Vídeo demonstrativo por exercício** — todo concorrente tem (400 a 12.000 vídeos); o FitCoach não. Escopar como campo de URL (YouTube) em `Exercise`, ou não entrar? | plano-de-negocio §5.5 | RF-EXE (novo) |
| 14 | **Adesão acionável**: alertar o treinador sobre aluno em risco de abandono (faltou X dias prescritos). É a aposta nº 2 da diferenciação, sem requisito escrito | plano-de-negocio §5.4 | RF-TRN (novo) |
| 15 | Avaliação física: dobras cutâneas em 3 ou 7 pontos (protocolo)? Sistema calcula % de gordura pela fórmula, ou só arquiva os valores brutos? | §9 | RF-AVA-11 |
| 16 | Avaliação física: feedback do aluno é só texto livre ou também nota/rating estruturado? É editável depois de enviado? | §9 | RF-AVA-12 |
| 17 | Avaliação física: limite de tamanho de arquivo (fotos) e tamanho/duração de vídeo (RF-AVA-10) — decisão técnica, deixada em aberto de propósito em 31 ago 2026 (a definir conforme o plano de VPS contratado) | §9 | RF-AVA-10 |

---

## 14. Matriz de rastreabilidade (fase → requisitos)

| Fase do roadmap | Requisitos que a fase entrega |
|---|---|
| **Já entregue (baseline)** | RF-AUTH-1 a 6 · RF-ALU-1 · RF-TRN-1, TRN-2 · RF-EXE-1 a 4 · RF-PLN-1 a 4 · RF-SES-1 a 5 · RNF-SEG-1 a 3 · RNF-DES-1 a 3 · RNF-CONF-2, CONF-3 · RNF-USA-3 · RNF-ARQ-1, ARQ-3, ARQ-4 |
| **Fase 1 — Backend** | RF-AUTH-7 · RF-CONV-1 a 4 · RF-ALU-2 a 6 · RF-EXE-5 · RF-PLN-5, PLN-8 · RNF-SEG-4, SEG-5 · RNF-CONF-1 · RNF-VAL-1, VAL-2 |
| **Fase 2 — Painel Web** | RF-CONV-5, CONV-6 · RF-ALU-7, ALU-8 · RF-TRN-3, TRN-4, TRN-5 · RF-EXE-6 · RF-PLN-6, PLN-7 · RNF-USA-1 |
| **Fase 3 — Mobile (aluno)** | RF-SES-6 a 9 · RF-GAM-14 (superfície) · RF-AVA-8, AVA-9 (superfície) · RNF-SEG-6 · RNF-LEG-1 |
| **Fase 4 — Watch** | RF-WCH-1 a 5 · RNF-USA-2 · RNF-ARQ-2 (conclusão) |
| **Backlog — Gamificação** (design em `gamificacao.md`) | RF-GAM-1 a 13 · RNF-DES-5 · RNF-LEG-3 |
| **Backlog — Avaliação física** (design em §9) | RF-AVA-1 a 7, AVA-10, AVA-14 · AVA-13 (backlog futuro, sem prazo) · RNF-LEG-4 |
| **Backlog — outros** | RF-EXE-7 · RF-FIN-1 a 4 |
| **Decisão de PO pendente** | RF-AUTH-8 · RF-TRN-7 · RF-PLN-9 · RF-GAM-5, GAM-10 · RF-AVA-11, AVA-12 · RNF-LEG-2 (ver §13) |

---

## Changelog

- **31 ago 2026**: corrigida numeração de subseções — `### 11.N` de RNF virou
  `### 12.N`, acompanhando o `## 12` do título (ficou desalinhado na
  renumeração anterior).
- **31 ago 2026**: revisão de gaps do módulo RF-AVA, a pedido do dono do
  produto — 6 pontos resolvidos:
  1. `RF-AVA-1` passa a capturar peso/altura por avaliação (sem isso não dava
     pra montar o histórico de evolução do RF-AVA-7) e sincroniza com o
     snapshot de `StudentProfile` (RF-ALU-1).
  2. Novo `RF-AVA-14` — editar avaliação depois de criada; exclusão fica fora
     de escopo (mesmo tratamento do RF-EXE-7), pra não quebrar o histórico.
  3. `RF-AVA-7` passa a exibir explicitamente o feedback do aluno (RF-AVA-9)
     na tela do treinador.
  4. Nova decisão #17 em §13: limite de tamanho/duração de fotos e vídeos
     (RF-AVA-10) — deixada em aberto de propósito, a definir conforme o plano
     de VPS contratado.
  5. Novo `RNF-LEG-4` — consentimento explícito do aluno antes de armazenar
     fotos/vídeos do corpo; `RNF-SEG-6` também passa a citar fotos/vídeos.
  6. Risco de custo de armazenamento de mídia registrado em
     `plano-de-negocio.md` §10.
- **31 ago 2026**: `RF-AVA-3` (bioimpedância) ganha 8 campos novos — massa gorda,
  massa óssea, água intracelular/extracelular (ICW/ECW), massa de proteína,
  idade metabólica, gordura visceral, análise segmentar (braço/tronco/perna) e
  pontuação/score do aparelho (campo livre, opcional) — completando a lista com
  o que aparelhos profissionais de bioimpedância (ex.: InBody) reportam, a
  pedido do dono do produto.
- **31 ago 2026**: adicionado `RF-AVA-13` — captura facilitada de bioimpedância
  (foto da tela do aparelho + OCR, e/ou integração via API com o fabricante da
  balança), pra substituir a digitação manual do `RF-AVA-3`. Pedido do dono do
  produto, explicitamente colocado em **backlog futuro** (sem prazo definido).
- **31 ago 2026**: resolvida a decisão de onde armazenar fotos/vídeos do
  `RF-AVA-10` — VPS Hostinger no início (restrição de orçamento), migração para
  nuvem de mercado (AWS/Azure/GCP) depois. Ver detalhe em
  [`roadmap.md`](./roadmap.md) "Fora de fase" e [`architecture.md`](./architecture.md)
  §7. Decisão #15 removida de §13 (resolvida); decisões #16 e #17 renumeradas
  para #15 e #16.
- **31 ago 2026**: `RF-AVA-10` estendido para incluir **vídeos** de
  acompanhamento/orientação (antes só fotos) — pedido do dono do produto.
  Prioridade sobe de `Could` para `Should` e sai de "decisão aberta" pra
  `backlog` (o que resta em aberto é só onde armazenar o arquivo, decisão #15
  de §13, que não bloqueia mais o escopo do requisito).
- **31 ago 2026**: adicionado o módulo **RF-AVA** (avaliação física e anamnese do
  aluno), resolvendo a decisão #14 que estava aberta em §13 desde 29 ago — escopo
  definido com o dono do produto: anamnese estruturada + três frentes de medida
  corporal (bioimpedância, dobras cutâneas/adipômetro, circunferências/fita
  métrica), histórico de avaliações e feedback do aluno sobre a avaliação
  recebida (`RF-AVA-1` a `12`). Renumeradas as seções WCH (§10), FIN (§11), RNF
  (§12), decisões (§13) e matriz (§14); `RNF-SEG-6` estendido para cobrir os
  dados de `RF-AVA`; 3 novas decisões em aberto (protocolo de dobras, fotos de
  acompanhamento, estrutura do feedback do aluno).
- **29 ago 2026**: três decisões de escopo novas (§12, #13–15) vindas da pesquisa
  de concorrência ([`plano-de-negocio.md`](./plano-de-negocio.md) §5): vídeo
  demonstrativo por exercício, avaliação física/anamnese e alerta de aluno em
  risco de abandono. Nenhum RF/RNF alterado — são lacunas ainda sem decisão.
- **27 ago 2026**: criação. Requisitos derivados de
  [`regras-de-negocio.md`](./regras-de-negocio.md),
  [`roadmap.md`](./roadmap.md) e [`architecture.md`](./architecture.md); nenhum
  requisito inferido diretamente do código (esses documentos já fizeram essa
  varredura em 20–25 ago 2026).
- **27 ago 2026**: adicionado o módulo **RF-GAM** (gamificação do aluno: streak
  por dia prescrito, dias treinados, conquistas, ranking) com base em
  [`gamificacao.md`](./gamificacao.md). Renumeradas as seções RNF (§11), decisões
  (§12) e matriz (§13); novos RNF-DES-5 e RNF-LEG-3; 4 novas decisões em aberto.
