# FitCoach — Roadmap

Documento de referência do produto: o que já existe, o que falta em cada fase, e o
backlog de temas futuros. É a base pra depois criar o GitHub Project (fases →
milestones, itens → issues). Nada daqui foi subido pro GitHub ainda.

Ver também: [`requisitos.md`](./requisitos.md) (requisitos numerados e rastreáveis,
com matriz fase → requisito), [`regras-de-negocio.md`](./regras-de-negocio.md),
[`plano-de-negocio.md`](./plano-de-negocio.md) (visão de produto e receita) e
[`gamificacao.md`](./gamificacao.md) (design da gamificação do aluno).

**Levantado em**: 25 ago 2026, verificado direto no código (não só na documentação
existente) — onde a documentação antiga (`architecture.md`, README) divergia do
código real, o código venceu e a divergência foi anotada.

## Como este roadmap funciona

- **Ordem das fases é fixa**: Backend → Front (painel web) → Mobile (app do aluno)
  → Watch. Cada fase só começa quando a anterior estiver 100% fechada — sem
  sobreposição. Decisão sua, registrada em 25 ago 2026.
- **Legenda**:
  - ✅ Feito e testado
  - ⬜ Pendente — falta implementar
  - 🔜 Bloqueada — depende da fase anterior fechar
  - ❓ Decisão em aberto — precisa de resposta antes de virar item de backlog concreto
- Cada item pendente vira uma issue quando formos criar o GitHub Project. Itens do
  Backlog (seção final) só serão detalhados depois que revisarmos o que foi feito.

---

## Fase 1 — Backend (API .NET) — 🚧 quase pronta

**Critério de "pronto" (combinado)**: todos os gaps funcionais corrigidos, CI/CD
rodando os testes a cada PR, **e** os itens de segurança abaixo resolvidos — o
backend sai desta fase já em condição de produção, não só funcionalmente completo.

### Feito e testado (61 testes: 30 unit + 31 integration)

| Área | Funcionalidade | Status |
|---|---|---|
| Auth | Login (trainer ou student) | ✅ |
| Auth | Cadastro de trainer | ✅ |
| Auth | Cadastro de student (vinculado ao trainer autenticado) | ✅ |
| Trainer | Dashboard (contadores + atividade recente) | ✅ |
| Trainer | Listar alunos / detalhe do aluno / histórico de atividade | ✅ |
| Exercícios | Listar (filtro por músculo/equipamento) | ✅ |
| Exercícios | Criar exercício próprio | ✅ |
| Planos | Listar / criar (com dias e exercícios aninhados) / detalhe | ✅ |
| Aluno | Dashboard (treino do dia, próximo treino, recordes, contadores) | ✅ |
| Sessões | Iniciar / registrar série / finalizar / detalhe | ✅ |
| Autorização | Isolamento por dono em todos os endpoints acima (IDOR coberto) | ✅ |

### Pendente pra fechar a fase

| # | Item | Detalhe |
|---|---|---|
| 1 | `PUT /api/plans/{id}` | Client (`plansApi.update`) já chama esse endpoint; API não o expõe. Sem ele, edição de plano é impossível em qualquer front. |
| 2 | Seed de exercícios globais | `SeedDefaultExercises` em `FitCoachDbContext.cs` monta a lista de exercícios padrão mas nunca chama `.HasData(...)` — a "biblioteca padrão" nunca é gravada no banco. Correção correta exige GUIDs fixos + migration nova. |
| 3 | Convite de aluno (substitui o `trainerInviteCode` atual) | **Decidido em 25 ago 2026**: convite único por aluno (link de uso único, com validade, marcado como usado depois do cadastro). Não substitui o cadastro direto — os dois fluxos convivem. Detalhe: (a) endpoint pro trainer gerar um convite (retorna link/código); (b) endpoint público de autocadastro do aluno via código de convite (sem JWT — diferente do `register/student` atual, que exige token de trainer); (c) cadastro direto atual (`POST /auth/register/student` autenticado) permanece como está, pra caso de aluno sem e-mail/cadastro presencial. Prazo de validade do convite fica a definir na implementação (sugestão: 7 dias). |
| 4 | CI/CD | Nenhum workflow do projeto em `.github/workflows` hoje (só os de dependências em `node_modules`). Rodar `dotnet test` a cada PR, no mínimo. |
| 5 | CORS de produção | `Program.cs` libera só `http://localhost:3000` e o placeholder `https://fitcoach.yourdomain.com`. Trocar pelo domínio real antes de o painel ir ao ar. |
| 6 | Tokens em cookie `httpOnly` | Hoje a API devolve `token` + `refreshToken` no corpo da resposta pro client guardar onde quiser; o `FitCoach.Web` guarda em `localStorage` (ver Fase 2, item espelhado). Mudar pra cookie `httpOnly` + `Secure` + `SameSite` é mudança de contrato API↔client — o lado API entra aqui. |
| 7 | Busca de aluno por CPF (evitar cadastro duplicado) | **Decidido em 25 ago 2026**: campo `cpf` obrigatório em `StudentProfile`, único por treinador (índice único composto `TrainerId`+`CPF` — o mesmo CPF pode existir sob treinadores diferentes, mas não duas vezes na carteira do mesmo treinador). Validar formato (11 dígitos + dígitos verificadores). Dois mecanismos: (a) endpoint de busca — `GET /api/students/search?cpf=...`, restrito ao trainer autenticado, pra checar antes de preencher o resto do cadastro; (b) validação de unicidade no momento de salvar, como rede de segurança. Aplica-se aos dois fluxos de criação de aluno: cadastro direto (já existente) **e** autocadastro via convite (item 3) — o aluno também informa o próprio CPF no formulário de convite, e passa pela mesma checagem. Exige migration nova. |
| 8 | Endpoint de edição do perfil complementar do aluno | Encontrado na varredura de 25 ago 2026 (`regras-de-negocio.md §4`): o DTO `UpdateStudentProfileRequest` (birthDate, weightKg, heightCm, goal, healthNotes) existe mas nenhum controller o usa — hoje não há nenhuma forma de editar esses dados depois do cadastro. |
| 9 | Configurar `FluentValidation` (validações de negócio ausentes) | Encontrado na varredura de 25 ago 2026 (`regras-de-negocio.md §10`): a dependência está no `.csproj` mas nunca foi registrada nem tem nenhum validator. Hoje não há validação de formato de e-mail, força de senha, nome mínimo, nem checagem de plano sem nenhum dia. Criar validators e registrar no `Program.cs`. |

---

## Fase 2 — Front (Painel Web do Treinador — Next.js) — 🔜 bloqueada pela Fase 1

### Feito

| Funcionalidade | Status |
|---|---|
| Dashboard do treinador (tela) | ✅ |
| Criar plano de treino (tela) | ✅ |
| Client de API tipado (`lib/api.ts`) cobrindo auth/students/exercises/plans/dashboard | ✅ |

### Pendente

| # | Item | Detalhe |
|---|---|---|
| 1 | Tela de login | Não existe nenhum arquivo de página hoje — `api.ts` já redireciona pra `/login` no 401, mas a rota não existe. |
| 2 | Lista de alunos | Não existe. |
| 3 | Detalhe do aluno (+ histórico de atividade) | Não existe. |
| 4 | Edição de plano de treino | Não existe. Depende do item 1 da Fase 1 (`PUT /api/plans/{id}`). |
| 5 | Migrar token de `localStorage` pra cookie `httpOnly` | Espelha o item 6 da Fase 1 — os dois lados mudam juntos. |
| 6 | Tela de biblioteca de exercícios | **Decidido em 25 ago 2026**: tela própria (não só o seletor embutido na criação de plano, que já existe e continua existindo). Escopo inicial segue o que a API já suporta: listar (com filtro por músculo/equipamento) e criar exercício próprio. Editar/excluir exercício fica de fora por ora — não há endpoint pra isso ainda (backlog futuro, sem issue aberta). |
| 7 | Ação no painel do treinador pra gerar/copiar link de convite | Provavelmente na lista de alunos ("+ Convidar aluno"). Chama o endpoint novo do item 3 da Fase 1. |
| 8 | Tela pública de cadastro do aluno via convite | Acessível sem login, a partir do link recebido. Aluno preenche os próprios dados (nome, e-mail, senha, **CPF**); vínculo com o trainer vem do código do convite. Ao final: se acesso for via navegador desktop, redireciona pra `/login`; se for mobile, mostra link pra baixar o app (Fase 3) — enquanto o app não existe, esse segundo caminho cai também no `/login` como fallback. |
| 9 | Busca por CPF + aviso de duplicidade | Campo de CPF com busca ao digitar (usa o endpoint de busca da Fase 1, item 7), tanto no formulário de convite (item 8) quanto no fluxo de cadastro direto do treinador (rota `/students/new` — confirmada na varredura de 25 ago 2026: o dashboard já linka pra ela no estado vazio "Nenhum aluno cadastrado ainda", mas a página não existe). Exibir aviso de duplicidade **só quando o CPF já existir na carteira do mesmo treinador** — CPF cadastrado sob outro treinador não conta como duplicado e não deve nem aparecer na busca (o endpoint da Fase 1 já é restrito ao trainer autenticado, então não há vazamento de "esse CPF já é aluno de alguém"). |
| 10 | Tela `/students/new` (cadastro direto de aluno) | Confirmada na varredura de 25 ago 2026: `dashboard/page.tsx` já linka pra `/students/new` no estado vazio, mas a página não existe — precisa ser criada (nome, e-mail, senha, CPF). |

---

## Fase 3 — Mobile (App do Aluno — React Native) — 🔜 bloqueada pela Fase 2

Nada implementado ainda. Funcionalidades já viabilizadas pela API (Fase 1), a
portar pro app:

| # | Funcionalidade | Depende de |
|---|---|---|
| 1 | Login do aluno (com credencial já criada — self-registration acontece via web, ver Fase 2 item 8) | Fase 1 (Auth) |
| 2 | Dashboard do aluno (treino do dia, próximo treino, recordes, sessões recentes) | Fase 1 (`/dashboard`) |
| 3 | Executar treino: iniciar sessão → registrar séries (peso/reps) → finalizar (FC média, calorias, notas) | Fase 1 (Sessions) |
| 4 | Histórico de sessões / detalhe de sessão | Fase 1 (`/sessions/{id}`) |
| 5 | Perfil do aluno (dados cadastrados pelo trainer — leitura) | Fase 1 (Students) |
| 6 | Tela de progresso: sequência (streak), dias treinados, conquistas e ranking | Backlog "Gamificação do aluno" (fatia de backend) + design em [`gamificacao.md`](./gamificacao.md) |
| 7 | Avaliação física: histórico das próprias avaliações (anamnese, medidas, evolução, fotos/vídeos) e envio de feedback sobre a avaliação recebida | Backlog "Avaliação física do aluno" (fatia de backend) + requisitos em [`requisitos.md`](./requisitos.md) §9 (RF-AVA-8, AVA-9, AVA-15) |

**Primeiro acesso resolvido (25 ago 2026)**: o app não precisa de tela própria de
cadastro — o aluno se cadastra pelo link de convite (Fase 2, item 8), que roda no
navegador (funciona antes mesmo do app existir). Uma vez com conta criada, o app
só precisa de login. Quando o app existir, o passo final do cadastro via convite
passa a redirecionar pra loja do app em vez de cair no fallback `/login`.

### Decisões em aberto antes de começar

- Dados sensíveis (`healthNotes`, FC, calorias e — quando o módulo de avaliação
  física existir — anamnese, medidas e fotos/vídeos do corpo): aplicar cache
  local com expiração, nunca logar, tráfego sempre em TLS (já apontado no
  `architecture.md §6`; requisito em `requisitos.md` RNF-SEG-6). As fotos/vídeos
  ainda exigem consentimento explícito do aluno (RNF-LEG-4).

---

## Fase 4 — Watch (watchOS + Wear OS via módulo KMP) — 🔜 bloqueada pela Fase 3

Nada implementado ainda.

| # | Item |
|---|---|
| 1 | Módulo `watch-shared` (Kotlin Multiplatform): modelos de sessão, cache local, lógica de sync |
| 2 | UI nativa watchOS (SwiftUI): iniciar/acompanhar treino, timer, FC |
| 3 | UI nativa Wear OS (Jetpack Compose): idem |
| 4 | Integração com HealthKit (watchOS) / Health Services API (Wear OS) para frequência cardíaca |

### Decisões em aberto antes de começar

- ❓ **Sync**: o relógio sincroniza direto com a API, ou sempre via phone como
  ponte Bluetooth? Só dá pra decidir com o app do aluno (Fase 3) já existindo.
- ❓ **Persistência local**: arquivo simples ou SQLite via KMP?

---

## Backlog (temas futuros — sem detalhamento ainda)

Combinado: só viram funcionalidades detalhadas depois que revisarmos o que foi
entregue nas Fases 1–4. Por ora, só os buckets herdados do README:

- **Avaliação física do aluno** — anamnese + medidas corporais (bioimpedância,
  dobras cutâneas/adipômetro, circunferências/fita métrica), com histórico de
  avaliações e gráfico de evolução. **Exceção ao "sem detalhamento" acima**:
  escopo definido com o dono do projeto em 31 ago 2026 — cobre anamnese
  estruturada, as três frentes de medida corporal citadas, histórico/evolução do
  aluno e feedback do aluno sobre a avaliação recebida (visível ao treinador).
  Trainer também pode enviar **fotos e vídeos** de acompanhamento/orientação por
  avaliação (vídeo é pedido novo, 31 ago 2026). Armazenamento **decidido em 31
  ago 2026**: VPS da Hostinger no início do projeto (restrição de orçamento),
  com migração planejada para nuvem de mercado (AWS, Azure ou GCP) depois — ver
  também "Fora de fase" abaixo. Precisa de fatia
  própria de backend (entidade `PhysicalEvaluation`, endpoints, testes) + tela de
  registro no painel web do treinador, a agendar; a visualização do histórico e o
  feedback do aluno aparecem na **Fase 3** (app do aluno), como a Gamificação.
  A bioimpedância (RF-AVA-3) entra digitada manualmente pelo treinador no MVP;
  uma forma mais fácil de capturar isso (foto da balança + OCR, e/ou API de
  algum fabricante que disponibilize uma) fica em **backlog futuro, sem prazo**
  — pedido do dono do produto em 31 ago 2026, RF-AVA-13.
  Requisitos numerados em [`requisitos.md`](./requisitos.md) §9 (RF-AVA).
- **Notificações push** (Firebase) — lembretes de treino.
- **Monetização / multi-academia** — assinatura digital, pagamentos (Stripe/Pagar.me), suporte a múltiplas academias.
- **Gamificação do aluno** — sequência (streak) de treinos prescritos, contador de
  dias treinados, conquistas e ranking entre alunos do mesmo treinador. **Exceção
  ao "sem detalhamento" acima**: o design técnico já está pronto em
  [`gamificacao.md`](./gamificacao.md) (escopo e regra de streak — "dia prescrito
  concluído" — definidos com o dono do projeto em 27 ago 2026). Precisa de uma
  fatia de backend própria (entidades, recompute, endpoints, testes), a agendar
  **depois da Fase 2**; aparece para o usuário na **Fase 3** (app do aluno).
  Requisitos numerados em [`requisitos.md`](./requisitos.md) §8 (RF-GAM).

---

## Fora de fase — a decidir onde entra

- 🟡 **Deploy/infraestrutura de produção** — **parcialmente decidido em 31 ago
  2026**: começa em **VPS da Hostinger** (restrição de orçamento), com migração
  planejada para nuvem de mercado (AWS, Azure ou GCP) depois — vale tanto pra API
  quanto pro armazenamento de mídia do RF-AVA (fotos/vídeos de avaliação física).
  Isso **diverge** do esboço de deploy 100% AWS que hoje está no README —
  README ainda não foi atualizado pra refletir a fase inicial na Hostinger.
  Ainda em aberto: se isso é parte do critério de "pronto" da Fase 1/2 ou uma
  fase própria de publicação, e o desenho de infra específico na Hostinger
  (banco gerenciado ou na própria VPS, CI/CD de deploy, etc.).

---

## Changelog deste documento

- **31 ago 2026**: varredura de consistência — Fase 3 ganha o item 7 (avaliação
  física no app do aluno), que a matriz de rastreabilidade de
  [`requisitos.md`](./requisitos.md) §14 já dava como entrega da fase mas a
  tabela daqui não listava; "Decisões em aberto" da Fase 3 atualizada pra citar
  anamnese, medidas e fotos/vídeos entre os dados sensíveis (acompanhando o
  RNF-SEG-6 estendido).
- **31 ago 2026**: adicionada ao backlog futuro (sem prazo) da Avaliação física
  a captura facilitada de bioimpedância — foto da balança + OCR e/ou API de
  fabricante, pra substituir a digitação manual do MVP. Detalhe em
  [`requisitos.md`](./requisitos.md) §9, `RF-AVA-13`.
- **31 ago 2026**: decisão de infraestrutura do dono do projeto — hospedagem
  começa em **VPS da Hostinger** (restrição de orçamento), com migração
  planejada para nuvem de mercado (AWS/Azure/GCP) depois. Resolve onde
  armazenar fotos/vídeos do RF-AVA-10 e atualiza "Fora de fase" (diverge do
  esboço 100% AWS do README, ainda não atualizado).
- **31 ago 2026**: **Avaliação física do aluno** ganha envio de fotos **e vídeos**
  de acompanhamento/orientação pelo treinador (antes só fotos) — pedido do dono do
  projeto. Detalhe em [`requisitos.md`](./requisitos.md) §9, `RF-AVA-10`.
- **31 ago 2026**: escopado o item de backlog **Avaliação física do aluno** —
  anamnese + medidas corporais (bioimpedância, dobras cutâneas/adipômetro,
  circunferências/fita métrica), histórico de avaliações e feedback do aluno
  sobre a avaliação recebida. Escopo definido com o dono do projeto; detalhado em
  [`requisitos.md`](./requisitos.md) §9 (novo módulo RF-AVA).
- **25 ago 2026**: criação. Fases e critérios definidos em conversa com o dono do
  projeto; estado de cada item verificado direto no código (`apps/backend`,
  `apps/web`), não só na documentação pré-existente.
- **25 ago 2026**: definido o fluxo de primeiro acesso do aluno — convite único
  por link (com validade), convivendo com o cadastro direto que o trainer já faz
  hoje. Atualizados Fase 1 (item 3), Fase 2 (itens 7 e 8) e Fase 3 (removida a
  decisão em aberto sobre primeiro acesso).
- **25 ago 2026**: adicionada busca de aluno por CPF pra evitar cadastro
  duplicado — único por treinador, obrigatório, com endpoint de busca prévia
  além da validação no salvar. Novo item 7 na Fase 1, novo item 9 na Fase 2.
- **25 ago 2026**: varredura completa do sistema (controllers, DbContext,
  services, telas web) pra gerar `regras-de-negocio.md` e fechar a lista de
  problemas conhecidos antes de criar as issues no GitHub. Achados novos:
  endpoint de edição de perfil do aluno inexistente (Fase 1, item 8),
  `FluentValidation` nunca configurado (Fase 1, item 9), e rota `/students/new`
  linkada pelo dashboard mas inexistente (Fase 2, item 10).
- **25 ago 2026**: confirmado que a biblioteca de exercícios (Fase 2, item 6)
  será uma tela própria, não só o seletor embutido na criação de plano.
- **27 ago 2026**: criados `plano-de-negocio.md` e `requisitos.md` (ver "Ver
  também" no topo). Adicionada **Gamificação do aluno** ao backlog, com design
  técnico completo em `gamificacao.md` (streak por dia prescrito, conquistas,
  ranking) — escopo definido com o dono do projeto. Nova linha 6 na Fase 3.
