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

`healthNotes` é texto livre pra observações rápidas do treinador — não confundir
com a anamnese estruturada planejada em §12, que é um dado à parte (por
avaliação, não por perfil) e não substitui esse campo.

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
- ❓ **Dobras cutâneas (§12)**: protocolo de 3 ou 7 pontos? O sistema calcula %
  de gordura pela fórmula do protocolo automaticamente, ou só arquiva os valores
  brutos digitados pelo treinador?
- ❓ **Feedback do aluno na avaliação física (§12)**: só texto livre, ou também
  nota/rating estruturado? É editável depois de enviado?
- ❓ **Limite de fotos/vídeos da avaliação física (§12)**: tamanho máximo de
  arquivo e duração máxima de vídeo — deixado em aberto de propósito em 31 ago
  2026, a definir conforme o plano de VPS contratado.

---

## 12. Avaliação física e anamnese do aluno (RF-AVA) — planejado, ainda não implementado

⚠️ Nada disto existe no código hoje — nenhuma entidade, endpoint ou tela. Épico
escopado com o dono do produto em 31 ago 2026; requisitos numerados e decisões
em [`requisitos.md`](./requisitos.md) §9 (RF-AVA), detalhamento de backlog em
[`roadmap.md`](./roadmap.md). Resumo dos campos planejados:

**Avaliação (`PhysicalEvaluation`)** — um registro datado por vez, vinculado ao
aluno e ao treinador que aplicou; um aluno pode ter várias ao longo do tempo.
Pode ser editada depois de criada; **exclusão fica fora de escopo** (mesmo
tratamento do §6, exercícios) — apagar quebraria o histórico de evolução.

| Campo | Obrigatório? | Regra planejada |
|---|---|---|
| `evaluatedAt` | Sim | data da avaliação |
| `studentId` / `trainerId` | Sim | mesma regra de autorização do resto do sistema (§9) — só o treinador dono do aluno acessa |
| `weightKg` / `heightCm` | Sim | peso/altura **na data desta avaliação** — não é o mesmo campo do §4 (`StudentProfile.WeightKg`/`HeightCm`, que é um snapshot único e mutável); ao salvar a avaliação, esse snapshot também é atualizado com os valores mais recentes |

**Anamnese estruturada** — não substitui o `healthNotes` livre do §4 (ver nota
lá); é um dado à parte, por avaliação.

| Campo | Obrigatório? | Regra planejada |
|---|---|---|
| Histórico de saúde | Não | texto livre |
| Lesões/cirurgias prévias | Não | texto livre |
| Doenças pré-existentes | Não | texto livre |
| Medicamentos em uso | Não | texto livre |
| Nível de atividade física atual | Não | texto livre |
| Restrições | Não | texto livre |

**Bioimpedância** — preenchida manualmente pelo treinador a partir da leitura do
aparelho; sem integração direta com hardware no MVP (captura facilitada por
foto+OCR ou API do fabricante é backlog futuro, sem prazo — RF-AVA-13).

| Campo | Unidade |
|---|---|
| % de gordura corporal | % |
| Massa gorda | kg |
| Massa magra | kg |
| Massa muscular | kg |
| Massa óssea | kg |
| Água corporal total | % |
| Água intracelular / extracelular (ICW/ECW) | % |
| Massa de proteína | kg |
| Taxa metabólica basal (TMB) | kcal |
| Idade metabólica | anos |
| Gordura visceral | índice/nível |
| Análise segmentar (gordura e massa magra por braço/tronco/perna) | — |
| Pontuação/score do aparelho | campo livre, opcional — proprietário de cada marca |

**Dobras cutâneas (adipômetro)** — em mm, protocolo Pollock 7 dobras: tricipital,
subescapular, axilar média, suprailíaca, abdominal, coxa, peitoral. Protocolo
(3 ou 7 pontos) e cálculo automático de % de gordura: ❓ ver §11.

**Circunferências (fita métrica)** — em cm: pescoço, ombro, tórax, cintura,
abdômen, quadril, braço (dir./esq., relaxado e contraído), antebraço (dir./esq.),
coxa (dir./esq.), panturrilha (dir./esq.).

**Fotos e vídeos de acompanhamento/orientação** — enviados pelo treinador.
Armazenamento: VPS da Hostinger no início do projeto (restrição de orçamento),
migração pra nuvem de mercado (AWS/Azure/GCP) planejada depois
(`architecture.md` §7). Exige **consentimento explícito do aluno** antes de
armazenar — dado de imagem do corpo, mais sensível que a anamnese/medidas.
Limite de tamanho de arquivo e duração de vídeo: ❓ decisão técnica deixada em
aberto de propósito, a definir conforme o plano de VPS contratado (ver §11).

**Feedback do aluno** — texto sobre a avaliação recebida, visível ao treinador
na tela de histórico/evolução do aluno (junto com a própria avaliação). Estrutura
do feedback (só texto ou também nota estruturada?): ❓ ver §11.

---

## Changelog

- **31 ago 2026**: revisão de gaps do §12 (RF-AVA), a pedido do dono do
  produto — `PhysicalEvaluation` passa a capturar peso/altura por avaliação
  (com sync pro snapshot do §4) e pode ser editada (exclusão fica fora de
  escopo); feedback do aluno passa a aparecer explicitamente na tela do
  treinador; fotos/vídeos exigem consentimento explícito do aluno; nova
  pergunta em aberto no §11 sobre limite de tamanho/duração de arquivo.
- **31 ago 2026**: adicionada a seção **12 — Avaliação física e anamnese do
  aluno (RF-AVA)**, planejado, ainda não implementado: anamnese estruturada,
  bioimpedância (13 campos), dobras cutâneas, circunferências, fotos/vídeos de
  acompanhamento e feedback do aluno. Duas novas perguntas em aberto no §11
  (protocolo de dobras, estrutura do feedback). Nota adicionada ao §4
  distinguindo `healthNotes` (observação livre) da anamnese estruturada (§12).
  Espelha [`requisitos.md`](./requisitos.md) §9 (RF-AVA).
- **25 ago 2026**: criação, levantado direto do código (Models, DTOs, Services,
  Controllers) do branch `chore/monorepo-restructure`.
