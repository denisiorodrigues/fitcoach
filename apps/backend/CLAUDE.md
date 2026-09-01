# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this directory (`apps/backend`).

Root-level conventions (docs-as-source-of-truth, commit/PR skills) are in the
repo-root `CLAUDE.md` — this file is backend-specific.

## Commands

```bash
docker compose up postgres        # from repo root — Postgres 16 on :5432, needed for anything below
dotnet build                      # whole solution (API + both test projects)
dotnet test                       # all tests (unit + integration)
dotnet test --filter "FullyQualifiedName~AuthServiceTests"        # one class
dotnet test --filter "FullyQualifiedName~AuthServiceTests.Login_WithValidCredentials_ReturnsToken"  # one test
cd FitCoach.API
dotnet run                        # API on the port in Properties/launchSettings.json; Swagger at /swagger
dotnet ef database update         # apply migrations
dotnet ef migrations add <Name>   # new migration after changing an entity or FitCoachDbContext
```

Integration tests run against `Microsoft.AspNetCore.Mvc.Testing` +
EF Core's InMemory provider (`FitCoachWebApplicationFactory.cs`), not a real
Postgres — `Program.cs` skips `Database.MigrateAsync()` when
`app.Environment.IsEnvironment("Testing")`.

## Solution layout

- `FitCoach.API` — the API itself.
- `FitCoach.UnitTests` — xUnit + Moq + FluentAssertions + Bogus fakes, references `FitCoach.API` directly.
- `FitCoach.IntegrationTests` — xUnit + `Microsoft.AspNetCore.Mvc.Testing` + EF InMemory, same reference.

## Architecture

Single `FitCoachDbContext` (`Data/FitCoachDbContext.cs`) across three domains:

- **Identity** — `User` (role, passwordHash) → `TrainerProfile` / `StudentProfile` (1:1, exactly one of each per user). `StudentProfile` belongs to one `TrainerProfile`.
- **Prescrição** — `Exercise` → `WorkoutPlan` → `WorkoutDay` → `PlanExercise`.
- **Execução** — `WorkoutSession` → `SessionSet`.

**Auth**: JWT Bearer, 7-day expiry + refresh token. Role (`Trainer`/`Student`)
and `profileId` are baked into the token at login/register and never change —
there's no role-switching. Controllers assert role with
`[Authorize(Roles = "Trainer")]` (or `"Student"`) at the class level.

**Ownership isolation (RNF-SEG-2)** — the pattern every endpoint follows, and
new endpoints must too: filter the query itself by the caller's `profileId`
(read from the JWT claims via `GetProfileId()`), and return `404` — not
`403` — when a resource exists but belongs to someone else. This is
deliberate: it never confirms to the caller that the resource exists at all.
See `StudentsController.cs` for the reference shape.

Controllers talk to `FitCoachDbContext` directly for reads; `AuthService` and
`WorkoutService` (both `Scoped`) hold logic for the write paths that need it
(registration/login, plan+session creation).

## Things to know before touching this code

- `FitCoachDbContext.SeedDefaultExercises` builds a list of default exercises
  but never calls `.HasData(...)` — it's dead code, nothing is actually
  seeded in the database yet (`docs/roadmap.md` Fase 1, item 2).
- The web client (`apps/web/src/lib/api.ts`) already calls
  `PUT /api/plans/{id}`, but that endpoint doesn't exist here
  (`docs/roadmap.md` Fase 1, item 1).
- A `trainerInviteCode` field exists on the student-registration DTO but the
  backend ignores it — the student's trainer link always comes from the
  authenticated caller's token, not from that field.
- `appsettings.json` has an S3 `Storage` section and the `.csproj` references
  `AWSSDK.S3`/`Azure.Storage.Blobs`, but no code uses them — media storage
  isn't implemented, and when it is, the plan is a Hostinger VPS first, not
  S3 (`docs/roadmap.md` "Fora de fase", `docs/architecture.md` §7).
