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
A **concorrência (§5) foi pesquisada em 29 ago 2026** — preços e recursos vêm das
páginas públicas dos produtos e mudam com o tempo. Tamanho de mercado e a
precificação do FitCoach seguem sem fonte, marcados `[a preencher — pesquisa]` ou
`[hipótese — validar]`: nesses pontos o documento entrega as perguntas certas, não
afirmações.

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
  inspiração no README — mas a pesquisa da §5 mostrou que ele mira academia/estúdio;
  o concorrente direto do autônomo é o **MFit Personal** (R$ 10,90–39,90/mês).
- **Diferenciação escolhida** (§5.4): atacar a **adesão do aluno**, problema que
  nenhum concorrente trata — gamificação da constância, adesão acionável no
  dashboard e relógio em **watchOS + Wear OS** (só o Wear OS é território livre;
  Apple Watch é paridade com o App Treino).

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
| Avaliação física do aluno: anamnese estruturada, medidas (bioimpedância, dobras cutâneas/adipômetro, circunferências/fita métrica), fotos e vídeos de acompanhamento/orientação, histórico de evolução e feedback do aluno | ⬜ backlog (RF-AVA, `requisitos.md` §9) |

**Sobre a avaliação física (RF-AVA) — o que ela realmente vale pro treinador**: é
valor real, não decorativo — viabiliza vender **consultoria online completa**
(não só plano de treino), dá credibilidade profissional na anamnese/medidas, e
sustenta cobrar mais do que o preço de entrada de planilha (§7). Mas a pesquisa de
concorrência (§5.3, item 2) mostra que App Treino, MFit e Tecnofit **já** entregam
avaliação física/composição corporal — então esse item **fecha um gap de table
stake** (§5.5), não abre vantagem competitiva sobre eles. O eixo em que o
FitCoach de fato se diferencia continua sendo a **adesão do aluno** (§5.4:
gamificação + adesão acionável + Wear OS) — RF-AVA é pré-requisito pra vender pro
segmento de consultoria online, não o motivo do treinador escolher o FitCoach em
vez do concorrente.

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
É a Fase 4 do roadmap.

⚠️ **Ajustado pela pesquisa de 29 ago 2026** (§5.3): "ter relógio" **não** é
diferencial — o App Treino já tem app de Apple Watch com HealthKit (FC, calorias,
distância) e sync em tempo real. O que é território livre é o **Wear OS**: nenhum
concorrente brasileiro pesquisado atende relógio Android. O argumento de marketing
correto é *"funciona também no seu relógio Android"*, não *"o único com relógio"*.

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

**Pesquisado em 29 ago 2026** a partir das páginas públicas dos produtos e de
comparativos do setor. Preços mudam — reconferir antes de decidir precificação.

### 5.1 Quem já está no mercado

| Produto | Foco | Preço público (ago 2026) | Sinais de escala |
|---|---|---|---|
| **App Treino** (apptreino.com.br) | Academia, estúdio, box de CrossFit, coach | Não divulga (venda B2B) | +2 mi de usuários em 2 anos, +48 mi de treinos executados, +76 mi de aulas agendadas, 4.8 de média em +300 mil avaliações |
| **MFit Personal** | Personal autônomo | Grátis (1 aluno) · R$ 10,90/mês (3 alunos) · R$ 39,90/mês (ilimitado); anual R$ 406,90 | 1.800+ vídeos de exercícios |
| **Tecnofit Personal** | Estúdio/academia com espaço físico | Grátis (básico) · a partir de R$ 189/mês | 500–600 vídeos; integra catraca |
| **TreinoAI** | Consultoria online com IA | R$ 24,90 a R$ 999,90/mês (por nº de clientes) | 400+ vídeos; periodização por IA |
| **Mobitrainer / Wiki4Fit** | Autônomo e estúdio de modalidades | A partir de R$ 29–29,90/mês | — |
| **Vedius / NextFit** | Biblioteca ampla / treino + nutrição | Sob consulta | Vedius: 12.000+ exercícios em vídeo |
| **Planilha + WhatsApp** | Autônomo iniciante | Grátis (custo é o tempo) | — |

### 5.2 Comparativo por critério

| Critério | FitCoach (alvo) | App Treino | MFit Personal | Tecnofit | Planilha + WhatsApp |
|---|---|---|---|---|---|
| Prescrição por dia da semana | ✅ funcional | ✅ | ✅ | ✅ | Manual |
| Biblioteca de **vídeos** de exercício | ❌ (só texto/músculo/equipamento) | ✅ | ✅ 1.800+ | ✅ 500+ | ❌ |
| Avaliação física / anamnese | ❌ | ✅ (com fotos) | ✅ | ✅ (antropometria, composição corporal) | Manual |
| Cobrança do aluno pelo app | ❌ (backlog) | ✅ cartão | ✅ (Carteira MFIT, PIX, boleto) | ✅ | ❌ |
| Agenda / check-in de aula | ❌ (fora de escopo) | ✅ | ❌ | ✅ | ❌ |
| **Dashboard de adesão do treinador** | ✅ funcional | Parcial (histórico do aluno) | Parcial (feedback do aluno) | Parcial | ❌ |
| Registro de série com progressão de carga | 🟡 API pronta | ✅ | ✅ | ✅ | ❌ |
| **Apple Watch** | ⬜ Fase 4 | ✅ (sync em tempo real + HealthKit: FC, calorias, distância) | ❌ | ❌ | ❌ |
| **Wear OS** | ⬜ Fase 4 | ❌ | ❌ | ❌ | ❌ |
| **Gamificação** (streak, conquistas, ranking) | ⬜ backlog | ❌ | ❌ | ❌ | ❌ |
| Preço de entrada | `[hipótese — validar]` | Sob consulta | R$ 10,90/mês | Grátis / R$ 189 | R$ 0 |

### 5.3 O que a pesquisa mostrou

1. **A âncora de preço do autônomo é baixíssima.** MFit cobra R$ 39,90/mês por
   alunos ilimitados; Mobitrainer e Wiki4Fit ficam em ~R$ 29. Qualquer preço acima
   disso precisa de justificativa forte. Isso invalida a hipótese original da §7 —
   ver revisão lá.
2. **Todo mundo compete no mesmo eixo**: tamanho da biblioteca de vídeos (400 →
   1.800 → 12.000), avaliação física e cobrança. É um eixo caro de alcançar e onde
   o FitCoach chega por último — **não é onde disputar**.
3. **App Treino não é concorrente direto do autônomo.** O posicionamento dele é
   academia/estúdio/box: agenda de aulas, check-in, catraca, lista de espera, rede
   social. Compete de fato com o Tecnofit, não com o MFit.
4. **Apple Watch já não é diferencial** — o App Treino tem, com HealthKit
   (frequência cardíaca, calorias, distância) e sync em tempo real.
5. **Wear OS é um buraco aberto.** Nenhum dos concorrentes brasileiros pesquisados
   tem app para Wear OS. Quem cobre relógio Android hoje é app genérico de
   registro de treino (Hevy, GymRun), sem vínculo com o personal.
6. **Ninguém fala de adesão do aluno como produto.** Todos vendem para o treinador
   *prescrever e cobrar*. Nenhum comparativo do setor cita streak, conquistas ou
   ranking. O problema "meu aluno parou de treinar" não tem dono.

### 5.4 Onde o FitCoach pode se destacar

A aposta não é ser um MFit mais completo — é atacar um problema que os outros não
tratam: **o aluno abandonar o treino**. É a dor que gera cancelamento do treinador,
e portanto o churn do próprio SaaS.

**Posicionamento**: *o app que faz o aluno treinar* — não o app que faz o personal
prescrever.

Três apoios concretos, em ordem de custo/benefício:

| # | Aposta | Por que se sustenta | Onde está |
|---|---|---|---|
| 1 | **Gamificação da constância** — streak por dia prescrito, dias treinados, conquistas, ranking da turma | Nenhum concorrente pesquisado tem. Custo baixo: sai do histórico de sessões que a API **já registra**, sem mudar o fluxo de treino | `gamificacao.md`; backlog com fatia própria após a Fase 2 |
| 2 | **Adesão como produto, não como relatório** — o dashboard já calcula taxa de adesão; falta agir sobre ela (quem faltou, quem está em risco, alerta ao treinador) | O treinador compra "meus alunos treinam mais", que é o que ele revende ao aluno | Dashboard ✅ funcional; alertas/risco = ⬜ a especificar |
| 3 | **Relógio nos dois sistemas, com foco em musculação** — watchOS **e Wear OS** | Wear OS é buraco aberto no mercado BR; Apple Watch é paridade com o App Treino, não vantagem | Fase 4 (`architecture.md §5`) |

**Correção de rota**: o "diferencial do relógio" precisa ser reescrito como
*"o único com Wear OS"*, não *"o único com relógio"* — a segunda afirmação é falsa.

### 5.5 O que falta ao FitCoach para ser considerado

Table stakes que **todos** os concorrentes têm e o FitCoach não:

| Lacuna | Peso na decisão do treinador | Situação |
|---|---|---|
| Vídeo demonstrativo do exercício | Alto — o aluno não sabe executar por texto | ⬜ não escopado |
| Avaliação física / anamnese | Alto para consultoria online | 🟡 escopado (RF-AVA, `requisitos.md` §9), não implementado |
| Receber pagamento do aluno pelo app | Alto para o autônomo | ⬜ backlog |

Sem pelo menos o vídeo, a troca dificilmente acontece — a diferenciação da §5.4
não compensa a ausência do básico. **Decisão pendente**: escopar vídeo de
exercício (link de YouTube por exercício já resolveria a v1, a custo baixo) —
não confundir com o vídeo de acompanhamento/orientação da avaliação física
(RF-AVA-10, §3.1), que é outro requisito, já escopado.

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

**Âncora de mercado (§5.1, ago 2026)**: MFit R$ 10,90 (3 alunos) e R$ 39,90
(ilimitado); Mobitrainer/Wiki4Fit ~R$ 29; TreinoAI a partir de R$ 24,90; Tecnofit
grátis no básico. O autônomo brasileiro está acostumado à faixa **R$ 10–40/mês**.

| Plano | Preço `[hipótese]` | Alunos ativos | Biblioteca própria | Relatórios | App do aluno | Multi-professor |
|---|---|---|---|---|---|---|
| **Free** | R$ 0 | até 3 | ✅ | Básico | ✅ | ❌ |
| **Pro** | R$ 29–39 `[hipótese]` /mês | ilimitado | ✅ | Completo + adesão | ✅ | ❌ |
| **Academia** | `[hipótese]` /mês | ilimitado (por professor) | ✅ | Completo + visão do estúdio | ✅ | ✅ |

Duas consequências da pesquisa:

- **O Pro tem que ser ilimitado.** Cobrar por faixa de alunos (hipótese original:
  "até 40") fica pior que o concorrente mais barato do mercado, que já dá
  ilimitado por R$ 39,90. Faixa por nº de alunos foi **descartada** para o Pro.
- **Não há espaço para preço premium hoje.** Premium exige o que o FitCoach ainda
  não tem (§5.5). Cobrar mais que R$ 39 só se sustenta depois do relógio (Fase 4),
  e mesmo assim como plano adicional, não como preço de entrada.

O plano Free existe para reduzir a barreira de adoção (o autônomo testa com
poucos alunos antes de pagar) e para alimentar o efeito de rede da §8. Free com
3 alunos empata com o MFit pago de entrada — é uma vantagem de aquisição.

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
| **Concorrente direto barato** (MFit a R$ 10,90–39,90/mês) domina o autônomo | Competitivo / teto de preço | Não competir em biblioteca de vídeo e cobrança; disputar o eixo de adesão do aluno (§5.4) |
| **Falta de table stakes** — vídeo de exercício e cobrança ainda sem decisão/backlog; avaliação física já escopada (RF-AVA) mas não implementada (§5.5) | Adoção travada mesmo com boa diferenciação | Vídeo de exercício: escopar o mínimo viável (link de YouTube). Decisão pendente. Avaliação física: priorizar a fatia de backend do RF-AVA no backlog |
| **A diferenciação é copiável** — streak e conquistas são baratos de imitar se derem resultado | Vantagem temporária | Usar a janela para acumular dado de adesão e hábito do aluno; o Wear OS (Fase 4) é a barreira mais cara de copiar |
| **Billing e multi-academia inexistentes** | Sem receita até serem construídos | Escopados no backlog do roadmap; priorizar após a Fase 2 |
| **Custo de armazenamento de mídia** (fotos/vídeos da avaliação física, RF-AVA-10) cresce com nº de treinadores × alunos × avaliações — a VPS Hostinger inicial tem orçamento limitado (`roadmap.md` "Fora de fase") | Custo de infra sobe antes da receita acompanhar, ou VPS fica sem espaço | Limite de tamanho/duração de arquivo (decisão técnica em aberto, `requisitos.md` §13); monitorar uso de disco; migração pra nuvem de mercado (AWS/Azure/GCP) já é o plano declarado quando a Hostinger não bastar |

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

- **31 ago 2026**: novo risco em §10 — custo de armazenamento de mídia
  (fotos/vídeos de RF-AVA) crescendo mais rápido que o orçamento da VPS
  Hostinger inicial; mitigação aponta pro limite de arquivo (decisão técnica
  em aberto) e pro plano já declarado de migração pra nuvem de mercado.
- **31 ago 2026**: explicitado em §3.1 o valor da **avaliação física (RF-AVA)**
  para o treinador — habilita vender consultoria online completa e sustenta
  preço mais alto, mas fecha um gap de table stake (§5.5) frente a App Treino,
  MFit e Tecnofit, não é o eixo de diferenciação (que segue sendo §5.4: adesão +
  gamificação + Wear OS). Envio de fotos/vídeos de acompanhamento pelo treinador
  entra no escopo do RF-AVA (vídeo é pedido novo do dono do produto). Atualizados
  o status da lacuna em §5.5 e o risco correspondente em §10.
- **29 ago 2026**: §5 reescrita com pesquisa real de concorrência (App Treino,
  MFit Personal, Tecnofit, TreinoAI, Mobitrainer, Wiki4Fit, Vedius, NextFit):
  preços, recursos e lacunas. Nova §5.4 com a tese de diferenciação (adesão do
  aluno: gamificação + adesão acionável + Wear OS) e §5.5 com os table stakes que
  faltam. Corrigido em §1 e §3.3 o "diferencial do relógio" — o App Treino já tem
  Apple Watch; o território livre é o Wear OS. §7 revista pela âncora de preço de
  mercado (Pro passa a ser ilimitado, faixa por nº de alunos descartada). §10
  ganha três riscos novos.
- **27 ago 2026**: criação. Fatos do sistema derivados de `architecture.md`,
  `roadmap.md`, `regras-de-negocio.md` e `manual-do-usuario.md`; dados de
  mercado, concorrência e preço deixados como pendência de pesquisa/validação.
- **27 ago 2026**: adicionada a **gamificação do aluno** como alavanca de
  retenção (§3.4) e as métricas de engajamento correspondentes (§9). Design
  técnico em `gamificacao.md`; requisitos em `requisitos.md` §8 (RF-GAM).
