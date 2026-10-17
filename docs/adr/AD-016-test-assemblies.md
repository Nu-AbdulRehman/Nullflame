# AD-016: Two test assemblies

**Date:** 2026-10-17 · **Closes:** BLOCKED question in TASK-B1-002 · **Decided by:** owner

## Decision
Tests live in two assemblies: `Nullflame.Tests.EditMode` (folder `Assets/_Project/Tests/EditMode/`, Editor platform only, references `Nullflame.Runtime` and `Nullflame.Editor`) and `Nullflame.Tests.PlayMode` (folder `Assets/_Project/Tests/PlayMode/`, all platforms, references `Nullflame.Runtime`). This replaces the "one for tests" wording in `CLAUDE.md` §3 and the single `Nullflame.Tests` assembly in IMPL §2.2.

## Rejected alternatives
One Editor-only assembly with play-mode checks written as EditMode `[UnityTest]`s that yield `EnterPlayMode`: rejected as slower and fragile across domain reloads. One all-platforms assembly: rejected because it can't reference the Editor assembly, so builder idempotency couldn't be tested.

## Consequences
Test Framework docs (1.4 manual, "Edit Mode vs. Play Mode tests") require Editor-only platforms for EditMode tests and all platforms for PlayMode tests, which is why one assembly can't hold both. `CLAUDE.md` §3 and `AGENTS.md` need their "one test assembly" wording updated (owner edit).
