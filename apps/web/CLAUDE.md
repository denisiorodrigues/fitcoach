# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this directory (`apps/web`).

Root-level conventions (docs-as-source-of-truth, commit/PR skills) are in the
repo-root `CLAUDE.md` — this file is web-specific.

## Commands

```bash
npm install
echo "NEXT_PUBLIC_API_URL=http://localhost:5000/api" > .env.local   # points at apps/backend, see its CLAUDE.md to run it
npm run dev      # localhost:3000
npm run build
npm run lint
```

No test setup exists in this app yet.

## Architecture

Next.js 14 App Router, TypeScript, Tailwind, TanStack Query. Only two real
screens exist today — everything else in the UI that looks like a link to
another page is a dead link (see `docs/manual-do-usuario.md` for the current
status of every screen):

- `src/app/(trainer)/dashboard/page.tsx` — trainer dashboard.
- `src/app/(trainer)/plans/new/page.tsx` — create workout plan.
- `src/app/(trainer)/layout.tsx` — shared trainer-section layout.

`src/lib/api.ts` is the single typed API client (axios): base URL from
`NEXT_PUBLIC_API_URL`, JWT injected from `localStorage` on every request via
an interceptor, auto-logout (clears storage, redirects to `/login`) on `401`.
`/login` doesn't exist as a page yet — the redirect target is a planned
route, not a bug in the client.

Token storage is `localStorage`, not an `httpOnly` cookie — known
pending hardening item, not something to "fix" incidentally
(`docs/requisitos.md` RNF-SEG-5, tracked as a paired API+web change since the
contract changes on both sides together).
