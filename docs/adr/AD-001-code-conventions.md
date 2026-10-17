# AD-001: Code conventions

**Date:** 2026-10-17 · **Closes:** OD-012 · **Decided by:** owner

## Decision
Use the IMPL §2 proposal: assemblies `Nullflame.Runtime`, `Nullflame.Editor` and `Nullflame.Tests`; namespaces `Nullflame`, `Nullflame.Logic`, `Nullflame.EditorTools`, `Nullflame.Tests`; folders `Assets/_Project/Scenes`, `Prefabs`, `Settings` and `Materials` alongside the folders `CLAUDE.md` fixes; one `GameContext.Current` static accessor that is set in `Awake` and cleared in `OnDestroy`.

## Rejected alternatives
Same names with references injected through spawner `Init` chains: rejected because every runtime prefab would need two or three hops of wiring for the same result.

## Consequences
IMPL §2.1, §2.2 and §2.4 are no longer proposals.
