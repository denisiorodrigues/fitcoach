---
name: commit-e-pr
description: >-
  Cria commit e abre o Pull Request do FitCoach seguindo as convenções do
  repositório. Use quando o usuário pedir para "commitar", "fazer commit",
  "abrir PR", "mandar PR", "subir as mudanças". Monta a mensagem no padrão
  Conventional Commits em PT-BR (feat/fix/docs/style/refactor/build/test/chore)
  com os trailers da sessão e cria o PR com o gh CLI. NÃO fatia commits, não
  roda testes, não cria branch e não faz push — em vez disso EXIBE lembretes
  para o usuário fazer esses passos.
---

# Commit e PR — FitCoach

Esta skill cobre **dois passos**: montar a mensagem de commit no padrão do repo e
abrir o PR com `gh`. Os demais passos (fatiar em commits lógicos, rodar testes,
criar branch, push) **não** são feitos automaticamente — a skill exibe lembretes.

## Passo 0 — Sempre exibir estes lembretes antes de commitar

Mostre este bloco ao usuário, literalmente, antes de criar o commit:

> ⚠️ **Antes do commit, confira você mesmo:**
> 1. **Um propósito por commit.** Se o diff mistura assuntos (ex.: fix + refactor
>    + docs), faça commits separados — rode a skill uma vez por fatia.
> 2. **Testou?** Se tocou código (`.cs`, `.ts`, `.tsx`), rode `dotnet test` no
>    backend antes de commitar. A skill não roda os testes.
> 3. **Branch.** Se está na `main`, crie uma branch antes
>    (`git checkout -b <tipo>/<descrição-curta>`). A skill não cria branch.
> 4. **Push.** Depois do commit, `git push -u origin <branch>` — o PR precisa do
>    branch remoto. A skill não faz push.

Depois de exibir, siga para o Passo 1 com o que já está staged (`git diff --cached`).
Se não houver nada staged, pergunte o que incluir (`git status`) — não rode
`git add -A` por conta própria.

## Passo 1 — Montar a mensagem de commit

Padrão: **Conventional Commits, assunto em português, no imperativo**.

`<tipo>(<escopo opcional>): <assunto>`

Tipos (do README, seção "Ajuda"):

| Tipo | Quando |
|---|---|
| `feat` | novo recurso (MINOR) |
| `fix` | correção de bug (PATCH) |
| `docs` | só documentação, sem código |
| `style` | formatação, lint, espaços — sem mudança de código |
| `refactor` | refatoração sem mudar comportamento |
| `build` | arquivos de build e dependências |
| `test` | criação/alteração de testes, sem mudar código de produção |
| `chore` | tarefas de build, config, pacotes |

- Assunto ≤ ~72 caracteres, sem ponto final.
- Corpo (opcional, separado por linha em branco): explique **o porquê** e o
  contexto, não o "o quê" (o diff já mostra). Quebre em ~72 colunas.
- **Trailers**: anexe os que a sessão manda anexar ao final da mensagem
  (`Co-Authored-By:` do Claude e `Claude-Session:` com a URL da sessão). Eles
  estão no system prompt da sessão — não invente a URL.

Exemplo (do histórico do repo):

```
fix(workout): corrige NRE ao criar plano e query não traduzível no dashboard

WorkoutDay.Exercises e WorkoutPlan.Days nunca eram inicializados, então
CreatePlanAsync estourava NullReferenceException sempre que o plano tinha
dias/exercícios. A query de Personal Records também não era traduzível pelo
EF Core; agora projeta os campos crus primeiro e agrupa em memória.

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: <url da sessão>
```

## Passo 2 — Commitar

- Rode `git commit` com a mensagem do Passo 1 (via `-m` repetido ou heredoc).
- Não use `--amend` a menos que o usuário peça.
- Mostre o `git log -1 --stat` do resultado.

## Passo 3 — Abrir o PR com `gh`

Pré-checagem:

- `git branch --show-current` — se for `main`, **pare** e diga:
  "Você está na `main`. Crie uma branch e faça push antes; eu não abro PR a
  partir da `main`." Não crie a branch você.
- Verifique se o branch tem upstream (`git rev-parse --abbrev-ref --symbolic-full-name @{u}`).
  Se não tiver, **pare** e diga o comando exato:
  `git push -u origin <branch>` — e peça para rodar a skill de novo depois.

Com branch remoto pronto:

```
gh pr create --base main --title "<mesmo padrão da mensagem de commit>" --body "<corpo>"
```

Corpo do PR:

```
## Resumo
- <1 a 3 bullets do que muda e por quê>

## Plano de teste
- <como verificar: dotnet test, passos manuais, etc.>

🤖 Generated with [Claude Code](https://claude.com/claude-code)

<link da sessão que a sessão manda anexar em corpos de PR>
```

- Se houver template de PR no repo (`.github/pull_request_template.md`), siga-o e
  encaixe as seções acima nele.
- Ao final, mostre a URL do PR que o `gh` retornou.

## Fora de escopo (é o que os lembretes do Passo 0 cobrem)

Fatiar commits · rodar `dotnet test` · criar branch · `git push`. A skill não faz
nenhum desses — só lembra.
