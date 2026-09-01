# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this directory (`apps/mobile`).

Root-level conventions (docs-as-source-of-truth, commit/PR skills) are in the
repo-root `CLAUDE.md` — this file is mobile-specific.

## Status: not started

`phone/`, `watch-ios/`, `watch-shared/`, `watch-wear/` currently hold only a
`.gitkeep` each — no code. This is Fase 3 (phone) and Fase 4 (watch) of
`docs/roadmap.md`, both blocked on Fase 2 (web panel) finishing first; do not
scaffold or add dependencies here unless the user is explicitly starting that
phase.

## Planned stack (`docs/architecture.md` §5)

| Dir | Stack | Role |
|---|---|---|
| `phone/` | React Native | App do aluno — mostly an API consumer (dashboard, plan of the day, session execution), reusing React knowledge from `apps/web` |
| `watch-shared/` | Kotlin Multiplatform (`commonMain`/`iosMain`/`androidMain`) | The only logic that must behave identically on both watches and work fully offline: session tracking, local cache, sync |
| `watch-ios/` | Swift/SwiftUI | Native watchOS UI, consumes `watch-shared` |
| `watch-wear/` | Kotlin/Jetpack Compose | Native Wear OS UI, consumes `watch-shared` |

Deliberate choice: UI is native per-platform, not shared via Compose
Multiplatform — only the tracking/cache/sync logic is shared, to avoid
depending on the least mature part of the KMP ecosystem for no real benefit
(the two watch UIs are already quite different).

Two things are explicitly undecided and block starting `watch-shared`
(`docs/roadmap.md` Fase 4 "Decisões em aberto"):

- Does the watch sync straight to the API, or always through the phone as a
  Bluetooth bridge?
- Local persistence: a flat file, or SQLite via KMP?
