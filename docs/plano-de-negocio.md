# FitCoach — Plano de Negócio

Camada de produto/negócio do FitCoach: para quem é, que problema resolve, como se
diferencia e como gera receita. Complementa os documentos técnicos e operacionais:

- [`requisitos.md`](./requisitos.md) — requisitos funcionais e não-funcionais rastreáveis;
- [`roadmap.md`](./roadmap.md) — fases de implementação e pendências;
- [`regras-de-negocio.md`](./regras-de-negocio.md) — o que cada campo/regra significa;
- [`gamificacao.md`](./gamificacao.md) — design da gamificação do aluno (retenção);
- [`architecture.md`](./architecture.md) — visão técnica e decisões de arquitetura;
- [`manual-do-usuario.md`](./manual-do-usuario.md) — como usar cada tela.

**Escrito em 27 ago 2026.** Os fatos sobre o sistema (funcionalidades, estado,
regras) vêm dos documentos acima, levantados direto do código em 20–25 ago 2026.
Os números de mercado, concorrência e preço ainda **não foram pesquisados** —
estão marcados como `[a preencher — pesquisa]` ou `[hipótese — validar]`. Este
documento entrega o esqueleto e as perguntas certas, não afirmações sem fonte.

---

## 1. Sumário executivo

FitCoach é um SaaS de gestão de treinos para **personal trainers** e **pequenas
academias/estúdios**. O treinador monta planos de treino por dia da semana
(exercícios, séries, repetições, carga, descanso), acompanha a adesão de cada
aluno num dashboard, e o aluno executa o treino registrando cada série — pelo
celular e, no futuro, pelo relógio.

- **Cliente pagante**: o treinador (ou a academia). O aluno não paga — o acesso
  dele está incluído na assinatura do treinador.
- **Modelo de receita**: assinatura mensal/anual, com faixas por número de alunos
  ativos; camada superior para academias com vários professores.
- **Referência de mercado**: App Treino (apptreino.com.br), citado como
  inspiração no README do projeto.
- **Diferencial de médio prazo**: acompanhamento do treino no **relógio**
  (Apple Watch + Wear OS) — treinar sem o celular na mão, com frequência cardíaca
  e timer.

**Estado atual do produto** (de `architecture.md §2`): a API (.NET 10 + PostgreSQL)
está funcional, com 61 testes automatizados; o painel web (Next.js) tem 2 telas
reais (dashboard do treinador e criação de plano); login, lista de alunos, detalhe
de aluno e edição de plano ainda não estão implementados; não existe app do aluno
(web nem mobile). O produto **ainda não é vendável** — ver §11 para o marco em que
passa a ser.

---

## 2. Problema e oportunidade

### 2.1 Dor do treinador
- Prescrição de treino hoje é feita em planilha, PDF, papel ou mensagens de
  WhatsApp — sem padronização e sem histórico estruturado.
- Não há visão de **adesão**: o treinador não sabe quais alunos estão realmente
  treinando sem perguntar um a um.
- Progressão de carga do aluno não fica registrada de forma consultável.
- Montar um plano novo para cada aluno, do zero, é repetitivo — falta uma
  biblioteca de exercícios reutilizável.

### 2.2 Dor do aluno
- Chega à academia sem saber o treino do dia (ou depende de abrir um PDF/foto).
- Perde o registro de quanto levantou na última vez — refaz a estimativa de carga
  de memória.
- Não tem visão da própria evolução (recordes, frequência).

### 2.3 Oportunidade de mercado
- Fitness em crescimento no Brasil, com forte presença de **personal trainers
  autônomos** e estúdios pequenos — segmento com baixa informatização.
- Tamanho de mercado (nº de personais registrados no CREF, nº de estúdios,
  ticket médio de ferramentas do segmento): `[a preencher — pesquisa]`.
- Ferramentas existentes ou são caras/complexas (foco em rede de academias) ou
  são genéricas (planilha) — há espaço para um produto simples, focado no fluxo
  treinador↔aluno.

---

## 3. Solução e proposta de valor

### 3.1 Para o treinador
| Entrega | Estado (ver `roadmap.md`) |
|---|---|
| Monta plano por dia da semana (dias → exercícios com séries/reps/carga/descanso), numa única tela | ✅ tela `/plans/new` funcional |
| Dashboard de adesão: total de alunos, ativos na semana, treinos prescritos, **taxa de adesão** (% de ativos) | ✅ funcional |
| Lista de alunos, detalhe do aluno e histórico de atividade | ⬜ Fase 2 |
| Biblioteca de exercícios com filtro por músculo/equipamento + exercício próprio | 🟡 API pronta; tela própria na Fase 2 |
| Convite de aluno por link (autocadastro) | ⬜ Fase 1 (backend) + Fase 2 (tela) |
| Edição de plano depois de criado | ⬜ Fase 1 (`PUT /api/plans/{id}`) + Fase 2 |

### 3.2 Para o aluno
| Entrega | Estado |
|---|---|
| Treino do dia e próximo treino (a partir do plano ativo e do dia da semana) | 🟡 API pronta; sem cliente |
| Executar sessão: iniciar → registrar cada série (peso/reps) → finalizar (FC média, calorias, notas) | 🟡 API pronta; sem cliente |
| Recordes pessoais (maior peso por exercício) e contadores de sessões | 🟡 API pronta; sem cliente |
| App mobile (React Native) | ⬜ Fase 3 |

### 3.3 Diferencial declarado
Acompanhamento do treino no **relógio** — módulo nativo watchOS (SwiftUI) + Wear OS
(Compose) sobre uma camada de lógica compartilhada em Kotlin Multiplatform
(`architecture.md §5`). O relógio registra a sessão offline e sincroniza depois.
É a Fase 4 do roadmap e o principal gancho de marketing frente a concorrentes que
só têm app de celular.

### 3.4 Retenção via gamificação (aluno)

O aluno engajado é o que segura a assinatura do treinador — se os alunos param de
treinar, o treinador cancela. A gamificação ataca isso recompensando constância:

- **Sequência (streak)** de treinos prescritos concluídos em dias seguidos;
- **Contador de dias treinados** (mês e acumulado);
- **Conquistas** por marcos (1º treino, 7/30 dias de sequência, 100 treinos…);
- **Ranking** entre os alunos do mesmo treinador.

Tudo é derivado do histórico de sessões que a API já registra — não muda o fluxo
de treino. Design técnico em [`gamificacao.md`](./gamificacao.md); requisitos em
[`requisitos.md`](./requisitos.md) §8 (RF-GAM). É um tema de **backlog** com uma
fatia de backend própria, a agendar após a Fase 2; aparece para o aluno na
Fase 3 (app).

---

## 4. Personas

O sistema tem dois papéis fixos (`UserRole`: Trainer e Student —
`regras-de-negocio.md §1`). As personas abaixo mapeiam nesses papéis.

| Persona | Papel | Contexto | Objetivo | Frustração hoje | O que o FitCoach entrega |
|---|---|---|---|---|---|
| **Rafael, personal autônomo** | Trainer | 15–40 alunos, atende em academia de rede ou a domicílio; usa planilha + WhatsApp | Escalar a carteira sem perder qualidade de acompanhamento | Não sabe quem está treinando; monta plano do zero toda vez | Dashboard de adesão, biblioteca de exercícios reutilizável, plano estruturado por dia |
| **Estúdio Corpo & Movimento** | Trainer (múltiplos) | Estúdio com 3–5 professores, ~120 alunos | Padronizar prescrição entre professores e ter visão do estúdio | Cada professor tem seu próprio método/arquivo; sem visão consolidada | Camada multi-academia (backlog): vários professores, visão agregada |
| **Marina, aluna** | Student | Treina 3–4×/semana, quer ver evolução | Chegar à academia sabendo o treino e a carga do dia | Depende de PDF/foto; não lembra a carga anterior | Treino do dia no celular/relógio, registro de série, recordes, histórico |

---

## 5. Concorrência

Comparativo de esqueleto — os campos dos concorrentes estão marcados `[a confirmar]`
enquanto não houver verificação direta.

| Critério | FitCoach (alvo) | App Treino | Planilha + WhatsApp | Outros apps de gestão `[a preencher]` |
|---|---|---|---|---|
| Prescrição por dia da semana | ✅ (alvo) | `[a confirmar]` | Manual | `[a confirmar]` |
| Dashboard de adesão do treinador | ✅ funcional | `[a confirmar]` | ❌ | `[a confirmar]` |
| App do aluno | ⬜ Fase 3 | `[a confirmar]` | ❌ | `[a confirmar]` |
| Registro de série / progressão de carga | 🟡 API pronta | `[a confirmar]` | ❌ | `[a confirmar]` |
| Integração com relógio (watchOS + Wear OS) | ⬜ Fase 4 (diferencial) | `[a confirmar]` | ❌ | `[a confirmar]` |
| Preço | `[hipótese — validar]` | `[a confirmar]` | Grátis (custo é o tempo) | `[a confirmar]` |

**Concorrente real do autônomo iniciante**: a planilha + WhatsApp. O produto
precisa ser mais rápido de usar que montar uma planilha, ou a troca não acontece.

---

## 6. Modelo de receita

- **Assinatura SaaS do treinador** — mensal ou anual (anual com desconto).
  Faixas de preço por **número de alunos ativos** na carteira.
- **Camada Academia** (backlog do roadmap: "multi-academia") — plano para estúdio
  com vários professores, cobrança por estabelecimento + faixa de alunos.
- **Aluno não paga** — acesso ao app do aluno está incluído na assinatura do
  treinador que o convidou.
- **Meios de pagamento** — Stripe e/ou Pagar.me, com assinatura recorrente
  (item do backlog de `roadmap.md`). Ainda não implementado.

Fontes de receita descartadas por ora: cobrança do aluno, anúncios, venda de
conteúdo de treino pronto (pode virar backlog, sem decisão).

---

## 7. Precificação (esboço)

Estrutura de planos — **todos os valores são hipótese e precisam ser validados**
(`[hipótese — validar]`).

| Plano | Preço `[hipótese]` | Alunos ativos | Biblioteca própria | Relatórios | App do aluno | Multi-professor |
|---|---|---|---|---|---|---|
| **Free** | R$ 0 | até 3 | ✅ | Básico | ✅ | ❌ |
| **Pro** | `[hipótese]` /mês | até 40 | ✅ | Completo | ✅ | ❌ |
| **Academia** | `[hipótese]` /mês | 40+ (faixas) | ✅ | Completo + visão do estúdio | ✅ | ✅ |

O plano Free existe para reduzir a barreira de adoção (o autônomo testa com
poucos alunos antes de pagar) e para alimentar o efeito de rede da §8.

---

## 8. Go-to-market

- **Canal inicial**: aquisição direta de personal trainers autônomos —
  Instagram/YouTube fitness, indicação boca a boca, grupos e comunidades de
  profissionais registrados no CREF, parcerias com cursos de formação.
- **Efeito de rede**: cada treinador que entra traz seus próprios alunos pelo
  fluxo de convite por link (`roadmap.md` Fase 1 item 3). O aluno tem uma
  experiência boa, vira personal ou indica para o próprio personal → laço de
  crescimento.
- **Landgrab do estúdio**: depois de tração com autônomos, subir para estúdios
  pequenos (3–5 professores) com a camada Academia.
- **Marcos de lançamento** amarrados às fases técnicas — ver §11.

Métricas de aquisição (CAC por canal, taxa de conversão Free→Pro, tempo até o
primeiro plano criado): `[a preencher — pesquisa]` / a instrumentar.

---

## 9. Métricas de negócio

| Métrica | Definição | De onde sai |
|---|---|---|
| Total de alunos | Alunos cadastrados na carteira do treinador | Dashboard do treinador — **já calculado** (`manual-do-usuario.md §1`) |
| Alunos ativos na semana | Treinaram nos últimos 7 dias | Dashboard do treinador — **já calculado** |
| Taxa de adesão | % de alunos ativos sobre o total | Dashboard do treinador — **já calculado** |
| Treinos prescritos | Total de planos criados | Dashboard do treinador — **já calculado** |
| MRR | Receita recorrente mensal | ⬜ a instrumentar (depende de billing) |
| Churn de treinador | % de treinadores que cancelam por mês | ⬜ a instrumentar |
| Conversão Free→Pro | % de contas Free que viram pagantes | ⬜ a instrumentar |
| CAC / LTV | Custo de aquisição / valor no tempo de vida | ⬜ a instrumentar |
| Ativação | % de treinadores que criam ≥1 plano e convidam ≥1 aluno na 1ª semana | ⬜ a instrumentar |
| Sequência média da turma | Média da sequência (streak) ativa entre os alunos de um treinador | ⬜ módulo de gamificação (`gamificacao.md`) |
| % de alunos com sequência ativa | Alunos com streak ≥ 1 sobre o total | ⬜ módulo de gamificação |
| Retenção D7 / D30 do aluno | % de alunos que treinam de novo 7 / 30 dias após o 1º treino | ⬜ a instrumentar |

As quatro primeiras já existem no produto e servem tanto ao treinador (valor de
uso) quanto ao negócio (proxy de engajamento). As demais dependem de billing,
analytics ou do módulo de gamificação, ainda não construídos.

---

## 10. Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| **Dados sensíveis de saúde** (`healthNotes`, frequência cardíaca, calorias) sob LGPD | Legal / reputacional | Cache local com expiração, sem dado sensível em log, TLS sempre; base legal e política de privacidade antes de captar aluno real (`architecture.md §6`) |
| **CREF do treinador** — hoje o cadastro não exige registro (`regras-de-negocio.md §11`) | Credibilidade / risco legal de habilitar leigo a prescrever treino | Decisão de PO pendente: exigir CREF, ou manter opcional com aviso de responsabilidade |
| **Senha do aluno no cadastro direto** — o treinador digita a senha; não há fluxo de primeiro acesso (`regras-de-negocio.md §11`) | Segurança / experiência ruim | Priorizar o fluxo de convite por link (aluno define a própria senha) |
| **Desenvolvedor solo** aprendendo mobile/watch do zero (`architecture.md`) | Prazo das Fases 3–4 imprevisível | Fatiar entregas; não bloquear receita nas fases mobile — o MVP vendável é a Fase 2 (§11) |
| **Sem CI/CD hoje** (`roadmap.md` Fase 1 item 4) | Regressão silenciosa ao crescer | Item obrigatório para fechar a Fase 1: `dotnet test` a cada PR |
| **Troca da planilha não acontece** se o produto não for mais rápido que montar planilha | Adoção baixa | Medir "tempo até o primeiro plano criado"; investir em UX da criação de plano |
| **Concorrente estabelecido** (App Treino) reage ou já cobre o nicho | Competitivo | Diferencial no relógio (Fase 4) + foco no fluxo simples treinador↔aluno; validar gap real na pesquisa da §5 |
| **Billing e multi-academia inexistentes** | Sem receita até serem construídos | Escopados no backlog do roadmap; priorizar após a Fase 2 |

---

## 11. Roadmap de negócio (resumo)

Não duplica o `roadmap.md` técnico — só liga cada fase ao marco de negócio.

| Fase técnica (roadmap) | Marco de negócio | O que passa a ser possível |
|---|---|---|
| **Fase 1 — Backend** fecha | Base sólida + segurança de produção | Publicar o painel sem risco (CORS, cookies httpOnly, CI); nada a vender ainda |
| **Fase 2 — Painel Web** fecha | **MVP vendável** | Treinador se cadastra sozinho, faz login, gerencia alunos, cria/edita planos, convida alunos por link. Começa a cobrança (Free/Pro) |
| **Fase 3 — App do aluno** fecha | Retenção | Aluno passa a usar diariamente; adesão sobe; reduz churn de treinador — reforçado pela gamificação (streak, conquistas, ranking — §3.4) |
| **Fase 4 — Watch** fecha | Diferencial de marketing | Gancho de aquisição frente a concorrentes; possível plano/preço premium |
| **Backlog — billing + multi-academia** | Escala de receita | Cobrança recorrente automatizada; entrada no segmento de estúdios |

---

## Changelog

- **27 ago 2026**: criação. Fatos do sistema derivados de `architecture.md`,
  `roadmap.md`, `regras-de-negocio.md` e `manual-do-usuario.md`; dados de
  mercado, concorrência e preço deixados como pendência de pesquisa/validação.
- **27 ago 2026**: adicionada a **gamificação do aluno** como alavanca de
  retenção (§3.4) e as métricas de engajamento correspondentes (§9). Design
  técnico em `gamificacao.md`; requisitos em `requisitos.md` §8 (RF-GAM).
