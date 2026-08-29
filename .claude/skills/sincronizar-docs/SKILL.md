---
name: sincronizar-docs
description: >-
  Atualiza os documentos em docs/ do FitCoach para refletir uma mudança de
  código. Use depois de implementar ou alterar uma funcionalidade, ou quando o
  usuário pedir "atualizar a documentação", "sincronizar os docs", "refletir
  isso na doc", "a doc ficou desatualizada". Decide quais docs/*.md são
  afetados, aplica as edições no mesmo tom dos docs e adiciona uma entrada de
  changelog datada em cada doc tocado.
---

# Sincronizar documentação — FitCoach

Mantém os docs de `docs/` em dia com o que o código realmente faz. Roda **depois**
de uma mudança de código (não gera código).

## Passo 1 — Entender a mudança

- Base: `git diff` (staged + unstaged) e, se já commitado, `git diff main...HEAD`.
- Se o usuário descreveu a mudança em vez de mostrar diff, use a descrição e
  confirme os pontos ambíguos antes de editar.
- Resuma em 1–2 frases o que mudou em termos de **comportamento observável**
  (endpoint, regra, campo, tela, status de fase) — é isso que a doc reflete, não
  a implementação.

## Passo 2 — Mapear mudança → documentos

| O que mudou no código | Reflita em |
|---|---|
| Endpoint novo/alterado, rota, contrato de request/response | `architecture.md` §4 · `regras-de-negocio.md` (seção do módulo) · `requisitos.md` (status do RF) · `README.md` seção "Rotas da API" |
| Regra de validação, campo obrigatório, limite de tamanho, enum | `regras-de-negocio.md` (seção do módulo + §10) · `requisitos.md` (RF ou RNF-VAL) |
| Model/entidade novo, relacionamento, migration | `architecture.md` §3 (modelo de dados) |
| Regra de autorização / visibilidade | `regras-de-negocio.md` §9 · `requisitos.md` RNF-SEG |
| Item de fase concluído ou repriorizado | `roadmap.md` (marcar/mover o item + changelog) · `requisitos.md` (status ✅/🟡/⬜ + matriz de rastreabilidade §13) |
| Tela ou fluxo novo no painel web | `manual-do-usuario.md` (status + passo a passo) · `requisitos.md` (RF-TRN/PLN/EXE) |
| Gamificação (streak, conquistas, ranking, recompute) | `gamificacao.md` · `requisitos.md` §8 (RF-GAM) |
| Decisão de produto, preço, persona, concorrência | `plano-de-negocio.md` |

Uma mudança quase sempre toca **mais de um** doc — `regras-de-negocio.md` e
`requisitos.md` costumam andar juntos.

## Passo 3 — Aplicar as edições no tom certo

- **Português**, frases curtas, tabelas quando couber. Não escreva parágrafos
  longos.
- Fatos vêm do código verificado agora — não copie o que a doc já dizia se o
  código mudou. Onde uma regra ainda não existe no código, marque como lacuna
  (`⬜`, "pendente", "❓ decisão em aberto"), não como se existisse.
- Status: `✅` implementado · `🟡` parcial (ex.: API pronta, sem tela) · `⬜`
  pendente. Use os mesmos símbolos em `roadmap.md`, `requisitos.md` e
  `manual-do-usuario.md` — e mantenha-os **consistentes entre si** (um RF que
  virou `✅` no `requisitos.md` não pode continuar `⬜` no `roadmap.md`).
- Ao marcar um item de fase como concluído no `roadmap.md`, verifique se isso
  fecha a fase (critério de "pronto" da fase) e sinalize se sim.

## Passo 4 — Changelog datado em cada doc tocado

Toda edição ganha uma linha no `## Changelog` do próprio doc:

```
- **<data de hoje, absoluta>**: <o que mudou e por quê, 1 linha>.
```

Converta datas relativas ("hoje", "ontem") para a data absoluta. Não invente
autoria.

## Passo 5 — Doc novo ou removido

Se a mudança **cria ou remove** um arquivo em `docs/`:

- Atualize a tabela **"Documentação"** do `README.md`.
- Atualize a lista de docs relacionados / "Ver também" no cabeçalho dos **outros**
  docs de `docs/`.

## Passo 6 — Revisão de consistência

Antes de terminar:

- Todo link cruzado (`./outro-doc.md`) aponta para arquivo que existe.
- Nenhum status conflita entre `roadmap.md` e `requisitos.md`.
- A matriz de rastreabilidade (`requisitos.md` §13) inclui os RF/RNF novos.
- `git status` mostra só arquivos de `docs/` e `README.md` — a skill não altera
  código.
