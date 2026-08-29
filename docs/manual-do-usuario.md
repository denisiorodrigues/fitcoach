# FitCoach — Manual do Usuário

Manual de uso do sistema, para treinadores e alunos. Cobre **como usar** cada
tela (cliques, campos, fluxo) — não confundir com:
- [`regras-de-negocio.md`](./regras-de-negocio.md) — o que é obrigatório e as
  regras por trás de cada campo;
- [`roadmap.md`](./roadmap.md) — o que falta implementar e em qual fase;
- [`requisitos.md`](./requisitos.md) — requisitos funcionais e não-funcionais
  numerados e rastreáveis;
- [`plano-de-negocio.md`](./plano-de-negocio.md) — visão de produto, mercado e
  modelo de receita;
- [`gamificacao.md`](./gamificacao.md) — design da gamificação do aluno (streak,
  conquistas, ranking).

**Levantado em 25 ago 2026.** Este manual reflete o estado real do sistema, não
o planejado — cada seção tem um status: ✅ disponível hoje, 🔜 ainda não
implementado, ❓ decisão de escopo pendente. Conforme as fases do roadmap forem
fechando, as seções 🔜 viram passo a passo real.

---

## Visão rápida

| Funcionalidade | Quem usa | Status |
|---|---|---|
| Acessar o sistema (login) | Treinador / Aluno | 🔜 sem tela — ver workaround abaixo |
| Dashboard do treinador | Treinador | ✅ |
| Criar plano de treino | Treinador | ✅ |
| Editar plano de treino | Treinador | 🔜 |
| Lista de alunos | Treinador | 🔜 |
| Detalhe do aluno | Treinador | 🔜 |
| Cadastrar aluno diretamente | Treinador | 🔜 |
| Convidar aluno (autocadastro) | Treinador cria, Aluno usa | 🔜 |
| Biblioteca de exercícios (tela própria) | Treinador | 🔜 |
| App do aluno (celular) | Aluno | 🔜 Fase 3 do roadmap |
| Relógio (watchOS / Wear OS) | Aluno | 🔜 Fase 4 do roadmap |

---

## Como acessar o sistema hoje (importante)

⚠️ **A tela de login ainda não existe.** O painel web (`dashboard` e `criar
plano`) já funciona, mas não há como entrar nele pela interface — é preciso
gerar um token manualmente pelo Swagger e colocá-lo no navegador. Isso é
temporário, só até a tela de login (roadmap Fase 2) existir.

1. Suba o ambiente: `docker-compose up -d` (ou rode API e Web separadamente,
   ver `README.md`).
2. Abra o Swagger da API: **http://localhost:5000/swagger**.
3. Cadastre um treinador em `POST /api/auth/register/trainer` (ou faça login em
   `POST /api/auth/login` se já tiver uma conta) e copie o valor de `token` da
   resposta.
4. Abra o painel web: **http://localhost:3000**. Abra o DevTools do navegador
   (F12) → aba **Console** e rode:
   ```js
   localStorage.setItem('fitcoach_token', 'COLE_O_TOKEN_AQUI')
   ```
5. Recarregue a página. Você já cai autenticado no dashboard.

Pra cadastrar alunos e exercícios (necessários pra criar um plano — ver
seção seguinte), use o Swagger da mesma forma: `POST /api/auth/register/student`
(com o token do treinador no botão "Authorize" do Swagger) e `POST
/api/exercises`.

---

## Para Treinadores

### 1. Dashboard — ✅ disponível

Tela inicial (`/dashboard`) depois de logado. Atualiza sozinha a cada 1 minuto.

**O que você vê:**
- 4 cartões de resumo: **Total de alunos**, **Ativos esta semana** (treinaram
  nos últimos 7 dias), **Treinos prescritos** (total de planos criados), e
  **Taxa de adesão** (% de alunos ativos sobre o total).
- Lista **"Atividade dos Alunos"**: até 8 alunos, ordenados pelo mais recente a
  treinar. Cada linha mostra nome, "há quanto tempo" foi o último treino (ou
  "Nunca treinou"), uma etiqueta **Ativo**/**Inativo**, e quantos treinos esse
  aluno fez no mês.
- Se não houver nenhum aluno cadastrado, aparece um estado vazio com o link
  "Cadastrar primeiro aluno".

**O que ainda não funciona nessa tela:**
- Clicar num aluno da lista tenta abrir `/students/{id}` (Detalhe do aluno) —
  página não existe ainda.
- O link "Ver todos" tenta abrir `/students` (Lista de alunos) — não existe.
- O link "Cadastrar primeiro aluno" tenta abrir `/students/new` — não existe.

### 2. Criar Plano de Treino — ✅ disponível

Acesse pela URL `/plans/new` (ainda não há um botão em nenhuma tela existente
que leve até aqui — ir direto pela URL).

**Pré-requisito**: pelo menos um aluno já cadastrado (via Swagger, ver seção
anterior) e pelo menos um exercício já cadastrado (idem — a biblioteca padrão
de exercícios ainda não foi semeada, [issue #4](https://github.com/denisiorodrigues/fitcoach/issues/4)).

**Passo a passo:**
1. **Informações do plano**: escolha o **Aluno** (obrigatório) e dê um **Nome
   do plano** (obrigatório, ex: "Hipertrofia 3x"). **Descrição**, **Início** e
   **Término** são opcionais.
2. **Dias de treino**: o plano já começa com um dia ("Treino A"). Clique em
   **"+ Dia"** pra adicionar mais (nomeados automaticamente Treino B, C...).
   Em cada dia, dá pra editar o **Rótulo** (ex: "Treino A") e o **Dia da
   semana**.
3. **Adicionar exercícios**: dentro de um dia, clique em **"Adicionar"** — abre
   uma busca com filtro por grupo muscular. Clique num exercício da lista pra
   incluir no dia.
4. Cada exercício adicionado vem com valores padrão (3 séries, 12 reps, 0 kg,
   90s de descanso) — edite **Séries**, **Reps**, **Carga (kg)**, **Descanso
   (s)** e, se quiser, uma **Observação do professor** pro aluno ver.
   Remova um exercício pelo ícone de lixeira.
5. Remova um dia inteiro pelo link "Remover este dia" (só aparece se houver
   mais de um dia).
6. Clique em **"Salvar Plano"**. Ao salvar, você é levado pra
   `/plans/{id}` — **essa página de detalhe do plano ainda não existe**, então
   hoje a confirmação de que salvou é só a ausência de erro; o plano já está
   no banco e visível via `GET /api/plans` no Swagger.

### 3. Login — 🔜 ainda não disponível
Planejado no roadmap, Fase 2. Até lá, use o workaround da seção "Como acessar
o sistema hoje".

### 4. Lista de alunos — 🔜 ainda não disponível
Planejado no roadmap, Fase 2.

### 5. Detalhe do aluno — 🔜 ainda não disponível
Planejado no roadmap, Fase 2 — vai incluir histórico de atividade.

### 6. Cadastrar aluno diretamente (`/students/new`) — 🔜 ainda não disponível
Planejado no roadmap, Fase 2 — vai incluir campo de CPF com busca de
duplicidade antes de salvar.

### 7. Convidar aluno (link de autocadastro) — 🔜 ainda não disponível
Planejado no roadmap, Fase 1 (backend) + Fase 2 (tela). O treinador vai gerar
um link único, o aluno se cadastra sozinho preenchendo nome, e-mail, senha e
CPF.

### 8. Editar plano de treino — 🔜 ainda não disponível
Planejado no roadmap, Fase 1 (backend, `PUT /api/plans/{id}`) + Fase 2 (tela).

### 9. Biblioteca de exercícios — 🔜 ainda não disponível
Confirmado como tela própria (não substitui o seletor embutido na criação de
plano, que já existe — ver item 2). Vai permitir listar exercícios com filtro
por músculo/equipamento e cadastrar exercício novo. Editar/excluir exercício
fica de fora por ora — a API ainda não suporta.

---

## Para Alunos

Nenhuma funcionalidade do aluno está disponível ainda — nem web, nem app. Tudo
abaixo é o planejado (roadmap, Fase 3):

### Cadastro — 🔜
Via link de convite enviado pelo treinador (ver item 7 acima), preenchido no
navegador antes mesmo do app existir.

### Login — 🔜
No app (React Native), com a conta já criada no cadastro por convite.

### Dashboard do aluno — 🔜
Treino do dia, próximo treino, recordes pessoais, histórico de sessões.

### Executar treino — 🔜
Iniciar sessão → registrar cada série (peso e repetições) → finalizar sessão
(frequência cardíaca média, calorias, observações).

---

## Relógio (watchOS / Wear OS) — 🔜

Planejado no roadmap, Fase 4 — tracking de treino no pulso (timer, frequência
cardíaca), com sincronização posterior com o app do aluno.

---

## Como este documento vai evoluir

Cada vez que uma funcionalidade marcada 🔜 for implementada, a seção
correspondente ganha o passo a passo real (e, quando fizer sentido,
screenshots) e o status muda pra ✅. As seções ❓ viram ✅ ou 🔜 assim que a
decisão de escopo for tomada.

## Changelog

- **25 ago 2026**: criação, cobrindo o estado real do sistema (2 telas web
  funcionais) e o restante planejado do `roadmap.md`.
