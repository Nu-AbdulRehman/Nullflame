# AGENTS.md — Nullflame

Brief for any coding agent working on this repo (Claude Code, Codex, Cursor, Copilot and others). It stays short on purpose. The full rules are in `CLAUDE.md`, the skill rules are in `SKILLS.md`, and the game itself is specified in `docs/design/nullflame-design.md`. Don't duplicate them here. Link to them.

Precedence when files disagree: design doc for *what the game does*; `CLAUDE.md` for *how the agent works*; this file is a summary of both. If you find a conflict, stop and tell the owner (`CLAUDE.md` section 2).

---

## Session start (mandatory, in this order)

1. Read `CLAUDE.md` in full, then `SKILLS.md`.
2. **Invoke `caveman` and `ponytail`.** In Claude Code: `/caveman full` then `/ponytail full`. In other tools, use that tool's skill call (for example `@caveman`, `@ponytail`). Confirm both are active before anything else. If either skill is missing, stop and send the owner a `BLOCKED` message with the install commands from `SKILLS.md`. Don't imitate a missing skill by hand.
3. Confirm MCP for Unity is connected to the right Editor (`set_active_instance` if more than one), then run `read_console` and report existing errors before changing anything.
4. At build step B1: confirm (read-only) that URP is the active render pipeline and list which approved asset packs are imported. Ask the owner to import any missing ones (`docs/assets/free-assets.md` §3).
5. Open the current build step's task list in `docs/tasks/B{n}.md`. If it doesn't exist yet, write it first (task-list procedure, `CLAUDE.md` 7.1).

Re-invoke both skills after any context reset or compaction, and whenever a new session starts.

---

## Project overview

- **Game:** third-person 3D stealth puzzle. You are a shadow creature that jumps between shadows, rides guards, consumes things, steals time powers and escapes a facility. Five puzzles in one level, 10–15 minutes.
- **Engine and target:** Unity 6, **URP**, desktop (Windows/macOS), keyboard and mouse.
- **Art:** approved free asset packs listed in `docs/assets/free-assets.md`, imported by the owner. Primitives only for gaps.
- **Constraint:** 12-hour game jam. The build order (design doc §15) is the schedule.
- **Tooling:** all Editor work goes through **MCP for Unity** (`https://github.com/CoplayDev/unity-mcp`). There is no command-line build or test runner in this workflow.

## Layout

| Path | Contents |
|---|---|
| `Assets/_Project/Scripts/` | Runtime code (one runtime assembly) |
| `Assets/_Project/Scripts/Logic/` | Plain C# rules with no scene dependencies, covered by EditMode tests |
| `Assets/_Project/Scripts/Editor/` | Builder scripts and editor tools (Editor assembly) |
| `Assets/_Project/Tests/` | EditMode and PlayMode tests (test assembly) |
| `Assets/_Project/Art/` | Prefab variants, material copies and the guard Animator Controller built on pack assets; the `AssetRegistry` |
| `Assets/_Project/Placeholders/` | Every `PH_*` placeholder asset (gaps only) |
| `Assets/ThirdParty/` and each pack's own import folder | Third-party assets. **Read-only.** Kept out of any public repo |
| `docs/design/nullflame-design.md` | Authoritative design document |
| `docs/assets/free-assets.md` | Approved asset packs, need-to-asset map, gap fallbacks, import steps |
| `docs/tasks/`, `docs/adr/`, `docs/OPEN-DECISIONS.md`, `docs/ENGINEERING-NOTES.md`, `docs/PLACEHOLDERS.md`, `docs/ASSET-MANIFEST.md` | Working docs (see `CLAUDE.md` section 1) |

Never touch `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/` or the MCPForUnity package files.

## The edit loop (run after every script change)

1. `create_script` for new files, `script_apply_edits` for changes (prefer it over `apply_text_edits`).
2. `validate_script`
3. `refresh_unity` with compilation
4. `read_console`. Fix every error and any new warning before the next change. Never stack edits on a broken compile.

## Testing

1. Enable the `testing` tool group with `manage_tools`.
2. `run_tests` (EditMode, and PlayMode where the task needs it).
3. Poll `get_test_job` until the job finishes. Report pass/fail counts. "Tests pass" without a finished job result doesn't count.
4. Every `Logic` change has EditMode tests. Every beat acceptance criterion (design doc §9) and tunable rule a task delivers has a test that names the beat or `T-*` id, or a manual Play-mode check you report.

## Conventions

- **Every number comes from `GameTuning`** (design doc §13). A new number needs a new `T-*` row approved by the owner.
- **Gameplay reads only `ShadowArea` components** for shadows, and only `FlameVolume` triggers for light damage. Lights are visuals.
- **Guard movement is deterministic** waypoint lerp with history sampled every 0.25 s. No randomness in anything that affects solvability.
- **Scene content is real GameObjects and prefabs** with Inspector fields. Nothing is built from scratch on Play. Bulk or parametric content (the facility greybox, pipe crack points) comes from an idempotent builder script listed in `docs/ASSET-MANIFEST.md`.
- **Two world-state roots** (`State_Meltdown`, `State_Normal`). Restart is a scene reload; `GameFlow.HasSeenIntro` is the only static that survives it.
- **No per-frame allocation** in `Update` (no LINQ, no new lists, no string building).
- **Assets:** every visual and sound goes through one `AssetRegistry` slot, pointing at a pack asset or a `PH_` placeholder. Pack models are child visuals of gameplay prefabs, which keep their own colliders and scripts. Kit pieces fit the §4 coordinates, not the reverse. Guard animation never drives position (root motion off).
- **Placeholders** (gaps only) are named `PH_<Purpose>`, use an approved pack material where possible, and get a `docs/PLACEHOLDERS.md` row in the same commit.
- **Third-party assets** get a row (slot, pack, source, license) in `docs/ASSET-MANIFEST.md` the first time they're used. The CC BY pocket watch needs its credit on the win or credits screen; credit Vefects for the fire too.

## Git and commits

- One task per commit. Message starts with the task ID: `TASK-B3-004: guard history sampling (S7, §8.2)`.
- Describe scene and prefab changes in plain words in the commit message; their YAML diffs are unreadable.
- Save every modified scene and prefab (`manage_scene`, `manage_prefabs`) before committing.
- Update affected docs in the same commit as the change.

## Definition of done (per task)

Clean console, all tests pass via a finished `run_tests` job, delivered beats and tunables are tested or manually checked, placeholders and third-party assets are logged, required credits are shown, docs are updated, and the end-of-task summary is sent (changes, test counts, new placeholders, open questions). A build step is done only when the game is playable at that step.

## Never, without the owner's explicit approval

- Guess. If it isn't written down, ask with the `BLOCKED` format (`CLAUDE.md` section 2). The design doc's `[Default]` items count as written down.
- Add, remove or upgrade packages (Cinemachine, ProBuilder, Timeline, Input System, TextMeshPro included).
- Switch away from URP, change Player, Graphics or Quality settings, the URP pipeline or renderer assets, or the target platform; start a build.
- Download anything, or use an asset that isn't on the approved list (Starter Assets and every listed fallback need approval first).
- Move, rename, edit or delete files inside a pack folder or `Assets/ThirdParty/` (make a prefab variant instead), or hand-fix pink pack materials (ask the owner to run the Render Pipeline Converter).
- Commit third-party pack files to a public repo.
- Use any `asset_gen` tool, or `execute_code` to change assets, scenes, settings or files (read-only inspection is fine, and say so).
- Hand-edit `.meta`, `.unity`, `.prefab`, `.asset` or `ProjectSettings/*` files.
- Delete or rename an existing scene, prefab or ScriptableObject.
- Build anything marked CUT, or apply the design doc's cut list yourself.
- Turn off `caveman` or `ponytail`, or let either one override the rules above (`SKILLS.md`).

If MCP for Unity disconnects or a tool keeps failing: retry once, then stop and report the error text. Never fall back to hand-editing Unity files.
