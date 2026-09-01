# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

FitCoach: a workout-management SaaS for personal trainers (.NET 10 API + Next.js
14 web panel today; React Native phone app and watchOS/Wear OS apps are planned,
not yet started). Monorepo, no root package manager/workspace tool — each app
under `apps/` is independent and run from its own directory, with its own
`CLAUDE.md` for stack-specific commands and architecture:

```
apps/
├── backend/   FitCoach.API (.NET 10) + tests — see apps/backend/CLAUDE.md
├── web/       Next.js 14 trainer panel      — see apps/web/CLAUDE.md
└── mobile/    phone/ watch-ios/ watch-shared/ watch-wear/ — see apps/mobile/CLAUDE.md (empty scaffolding, Fase 3–4)
packages/config/  shared JS/TS config (eslint, tsconfig) — empty, Fase 4
docs/             requirements, roadmap, business rules — see below
```

## Docs are the source of truth

`docs/` is written and kept current *before/alongside* code, and it says so
explicitly when something is planned but not built — don't infer a feature
exists from a doc mentioning it. Status symbols are used consistently across
docs: `✅` implemented · `🟡` partial · `⬜` pending.

- `docs/requisitos.md` — numbered requirements (`RF-<módulo>-N` /
  `RNF-<categoria>-N`), MoSCoW priority, status, which roadmap phase delivers
  each one, and a decisions-still-open log (§13).
- `docs/roadmap.md` — phases in fixed order (Backend → Web → Mobile → Watch,
  no overlap by design) with the concrete pending items per phase.
- `docs/regras-de-negocio.md` — field-by-field dictionary (required-ness,
  limits, authorization rules) reflecting actual code state.
- `docs/architecture.md` — technical decisions and known gaps.
- `docs/github-project.md` — how the above maps to a GitHub Project
  (milestones = phases, issue granularity); nothing has been created on
  GitHub from it yet.

Two skills are set up for this repo's workflow:

- **`sincronizar-docs`** — run after implementing or changing behavior, to
  update the affected docs (it maps code changes → which docs to touch) and
  add a dated changelog entry to each. Docs drifting from code is treated as
  a bug here, not cosmetic.
- **`commit-e-pr`** — commit message and PR conventions for this repo:
  Conventional Commits with a Portuguese, imperative subject
  (`feat`/`fix`/`docs`/`style`/`refactor`/`build`/`test`/`chore`, per the
  README's "Ajuda" section), one purpose per commit.
