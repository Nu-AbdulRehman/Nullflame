# CLAUDE.md — Nullflame

Read this whole file at the start of every session. It overrides your defaults. Then read `SKILLS.md` and invoke the mandatory skills (section 7) before any other work. `AGENTS.md` is a short summary of this file for other coding agents.

---

## 1. What this project is

**Nullflame** is a third-person 3D stealth puzzle game for **desktop (Windows/macOS, keyboard and mouse)**, built in **Unity 6** during a **12-hour game jam**. All Unity work is done by a Claude agent through MCP for Unity.

You play a newly discovered SCP: a living patch of nothingness that can only exist inside shadows. A facility meltdown pops every containment door open. You jump between shadows, ride guards' moving shadows to cross rooms, consume guards and objects through their shadows, steal the Time SCP's power, reverse a guard's walk to carry you where you need to go, rewind the facility from Meltdown to Normal at a breaker lever, and finally consume the Final Gate. Flame light (guard lanterns, alarm lights, wall fire lanterns) burns you even inside a shadow; a guard who spots you throws a lantern that becomes a spreading fire. One bar, **Essence**, is both health and energy. Death restarts the whole game. One continuous level, five puzzles, 10 to 15 minutes for a first clear.

Design goals: one strong mechanic (living in shadows), art from approved free asset packs with primitives only for gaps, every number tunable from one asset, and a build order where each step is playable before the next.

### Source-of-truth documents

| File | What it holds |
|---|---|
| `docs/design/nullflame-design.md` | The game design document: core rules (§2), controls (§3), facility coordinates (§4), mechanics (§5–§8), puzzle beats with acceptance criteria (§9), world states (§10), tuning table `T-*` (§13), systems `S1`–`S20` (§14), build order and cut list (§15), defaults and open questions (§16). **Authoritative.** |
| `docs/tasks/B{n}.md` | Stripped task list per build step (design doc §15, steps 1–10), produced by the task-list procedure (section 7.1). Task IDs `TASK-B{n}-{nnn}`. |
| `docs/OPEN-DECISIONS.md` | Decisions waiting on the owner. IDs `OD-{nnn}`. |
| `docs/adr/AD-{nnn}-<slug>.md` | Decisions the owner has made. IDs `AD-{nnn}`. |
| `docs/ENGINEERING-NOTES.md` | Small, local, reversible choices and why they were made. |
| `docs/PLACEHOLDERS.md` | Every placeholder asset and what the final asset must be. IDs `PH-{nnn}`. |
| `docs/assets/free-assets.md` | The approved free asset packs, the need-to-asset map, gap fallbacks and import steps (section 5). Copy of the project's asset list. |
| `docs/ASSET-MANIFEST.md` | Every builder script and the content it constructs (section 4.1), and every third-party asset used with its source and license (section 5.4). |
| `SKILLS.md` | Mandatory skills (`caveman`, `ponytail`), how to invoke them, and how they rank against these rules. |
| `AGENTS.md` | Short brief for any coding agent, summarising this file. If it disagrees with this file, this file wins; report the conflict. |

ID conventions: systems are `S1`–`S20`, tunables are `T-<AREA>-<NAME>` (for example `T-ESS-MAX`), puzzle beats are `Beat 0`–`Beat 5`, design doc sections are written `§n`. Task IDs start with `TASK-` so they never collide with tunable IDs.

If any of these files is missing when you need it, **ask the owner** whether to create it. Don't silently create a different structure.

---

## 2. The golden rule: never assume, ask

**If you can't find something, or the answer isn't written down, stop and ask the owner. Do not guess, not even "temporarily", not even for implementation details.**

Items the design doc marks `[Default]` **are** written down: build them as specified and don't re-ask them. They stay open to the owner overruling them (§16.2).

This applies to:
- Requirements that are ambiguous, missing, or contradict each other (including a contradiction between a beat's acceptance criteria and a mechanic section).
- Anything in design doc §16 that is still open, and any new question you find, until it has an `AD-*` record.
- Unity version, packages, project settings, input system, post-processing, build settings. (The render pipeline is decided: URP.)
- Unity APIs you aren't certain exist in the pinned Unity 6 version. First check with MCP for Unity's `unity_docs` or `unity_reflect`. If that doesn't settle it, ask.
- Files, folders, scenes, assets or config the task refers to but you can't locate.
- Naming, folder placement or architecture not covered by the design doc or this file.
- Positions and sizes not given in design doc §4. Never eyeball a coordinate that the schematic doesn't give.

**The only exception is missing art, audio, font or VFX assets.** Those come from an approved pack, or get a placeholder under the rules in section 5, and the owner is told afterwards. A pack that isn't imported yet is not an exception: ask the owner to import it.

Asking is done through the owner-question procedure (section 7.2). Its output uses this format, and you stop working on the blocked item until it's answered:

```text
BLOCKED: <what you need, one line>
Task: <TASK-B*-nnn>, relates to <S*/T-*/Beat/AD/OD ids, design doc §>
Looked at: <files, docs, MCP queries you checked>
Options I can see: <2+ options with one-line trade-offs, no pick>
Needed from you: <the exact decision or file>
```

You may continue with *other* tasks that don't depend on the answer. Say which ones you're continuing with. This is a 12-hour jam: always keep something unblocked moving.

---

## 3. Architecture rules

**Do**
- Keep runtime code in `Assets/_Project/Scripts/` with one assembly definition for runtime, one for `Editor` code, and one for tests.
- Put rules that don't need the engine in **plain C# classes** under `Assets/_Project/Scripts/Logic/`, owned and called by thin MonoBehaviours. At minimum: Essence spending and floor rules (§6), guard path history and reverse playback (§7.4, §8.2), the pipe crack meter zones (§7.6), the spotting grace timer (§8.3), and the fire radius over time (§7.9).
- Read every tunable from the single `GameTuning` ScriptableObject (§13). No magic numbers in gameplay code. A number not in §13 gets a new `T-*` row proposed to the owner before it's used.
- Gameplay reads **only `ShadowArea` components** (§7.1) for shadows, never real-time shadow maps or light raycasts. Lights are for looks; damage comes from `FlameVolume` trigger volumes (§7.7).
- Keep guard movement deterministic: waypoint lerp, history sampled every 0.25 s while travelling (§8.1, §8.2). Reversing a guard must retrace its exact path.
- Build the two world states as two parent objects, `State_Meltdown` and `State_Normal`, with shared geometry outside both (§10). The facility rewind swaps them; nothing else toggles them.
- Restart by reloading the `Facility` scene (§14). `GameFlow.HasSeenIntro` is the only static state that survives a reload.
- Write EditMode tests for every `Logic` class change. Every beat acceptance criterion (§9) and every tunable rule the task delivers needs at least one test (EditMode or PlayMode) that names the beat or `T-*` id in the test name or a comment. If a criterion can't reasonably be automated, write it as a manual check in the task list and report its result.
- Author scene content as real GameObjects and prefabs (section 4.1). Runtime code configures and animates them; it does not construct them from nothing.
- Unsubscribe events and stop coroutines in `OnDisable`/`OnDestroy`.

**Don't**
- Don't let `Logic` classes depend on scene objects, `MonoBehaviour`, or `Time.deltaTime` directly. Pass time and positions in.
- Don't use randomness in guard routes, spawn timings, view cones or anything that affects puzzle solvability. Cosmetic randomness (flicker, particles) only.
- Don't allocate per frame in `Update` loops (no LINQ, no new lists, no string building in hot paths). Guard vision and shadow queries run every frame.
- Don't add, remove or upgrade any package without the owner's approval. This includes Cinemachine, ProBuilder, Timeline, the Input System, TextMeshPro, tweening libraries, test packages and analyzers. Where the design doc offers "X or custom" (camera §11, greybox §14), use the option that needs no new package until the owner approves one.
- **The render pipeline is URP** (owner-approved with the asset packs, 2026-10-17). Don't switch it, and don't change Player Settings, Graphics/Quality settings, the URP pipeline or renderer assets (including renderer features), or the target platform without approval. The time-power screen effect (§7.4) and damage vignette (§12) are built with a URP Volume in the scene; ask before any change they'd need to the pipeline asset.
- Don't implement anything marked **CUT** in the design doc (speed up, stop time), and don't apply the §15 cut list on your own. Cutting is the owner's call; ask. Never propose cutting anything on the §15 "Never cut" list.

---

## 4. MCP for Unity rules

All Unity Editor work goes through **MCP for Unity** (`https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`, docs: https://coplaydev.github.io/unity-mcp/). Tool names below come from its tool reference. If a tool behaves differently from what's described here, trust the live tool description and tell the owner the docs here are out of date.

**Do**
- **At session start:** after invoking the mandatory skills (section 7), confirm the MCP connection works and you're targeting the right Editor. If more than one instance is open, use `set_active_instance`. Then run `read_console` and report any existing errors before changing anything. At the start of build step B1, also confirm (read-only) that URP is the active pipeline and list which approved packs are imported; ask the owner to import any missing ones the step needs.
- **After every script create or edit:** run `validate_script`, then `refresh_unity` with compilation, then `read_console`. Fix all compile errors and any *new* warnings before making another change. Never stack edits on a broken compile.
- Prefer `script_apply_edits` (structured, method or class level) over `apply_text_edits` for C# changes. Use `create_script` for new files.
- Use `batch_execute` when making several related scene, GameObject or component changes.
- Verify Unity APIs against the pinned version with `unity_docs` or `unity_reflect` before using any API you aren't sure of.
- Enable optional tool groups only when you need them (with `manage_tools`), such as `testing` for `run_tests`, `ui` for UI Toolkit, or `vfx` for `manage_texture` and particles. Turn them off again when done.
- Run tests with `run_tests`, poll `get_test_job` until the job finishes, and report the pass/fail counts. Don't report "tests pass" without a finished job result.
- Save every scene and prefab you modify (`manage_scene` / `manage_prefabs`). Unsaved Editor state is invisible to git and is lost.
- Describe scene and prefab changes in plain words in the commit message, since their YAML diffs are unreadable.

**Don't**
- **Don't use any `asset_gen` tool** (`generate_image`, `generate_audio`, `generate_model`, `import_model`, `import_model_file`). They cost money, call external services and raise licensing questions. Use the approved packs or placeholders (section 5) instead. The two non-store models are added by the owner by hand. Only use them if the owner explicitly approves a specific call.
- Don't use `execute_code` to create or change assets, scenes, settings or files. Changes made that way can't be reproduced from source. Read-only inspection is allowed; say in your message that the call was read-only.
- Don't use `manage_packages` to install, remove or upgrade packages without approval. Querying is fine.
- Don't use `manage_build` to switch platforms or start builds without asking first. Switching platforms triggers a long reimport, which a jam can't afford.
- Don't use `manage_graphics` or `manage_editor` to change project-wide settings without approval.
- Don't hand-edit `.meta`, `.unity`, `.prefab`, `.asset` or `ProjectSettings/*` files with plain file tools. Go through MCP for Unity.
- Don't touch `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, or the MCPForUnity package files. Treat third-party pack folders and `Assets/ThirdParty/` as read-only (section 5.1).
- **If MCP for Unity is disconnected or a tool keeps failing:** retry once. If it still fails, stop and tell the owner what failed and the error text. Don't fall back to hand-editing Unity files.

### 4.1 Building scene content

Editor API calls for one-off objects; a recorded, idempotent builder script for anything bulk, parametric or rebuildable from a config asset; the scene and prefab serialisation format is never hand-edited.

**The owner must be able to open the `Facility` scene, see every object, and configure it in the Inspector.** So:

- **Never construct content at runtime that can exist as a GameObject or prefab.** No single bootstrap script that builds the game on Play. Systems that need runtime state are **manager GameObjects** in the scene (for example `GameFlow`, `WorldStateController`, `ShadowRegistry`, the HUD), each with serialized references to the objects it drives.
- **Configurable objects are prefabs with serialized parameters.** A guard is a prefab whose route (`WaypointRoute` reference), lantern color, Post behaviour and starting state are Inspector fields. A `ShadowArea` is a prefab whose opacity, bounds and consumable reference are Inspector fields, and it previews its footprint in Edit mode. `FlameVolume`, `Consumable`, `CrackablePipe`, `GuardGate` and `GuardSpawner` follow the same pattern.
- **Waypoint routes are scene objects** (a parent with child waypoint transforms), visible as gizmos in Edit mode, so the owner can drag a route and replay the beat.
- **Objects that must be created at runtime are instantiated from prefabs** (spawned guards, oil drops, void shadows, lantern fire, thrown lanterns) and configured through the same parameters, never assembled component by component in code.
- **UI and HUD follow the same discipline.** The Essence bar, prompts, hold ring, tutorial hints, title, death and win screens are GameObjects and prefabs in the scene, not generated at runtime.
- **Visual models come from the `AssetRegistry`** (section 5.2), as child visuals of gameplay prefabs. Gameplay prefabs keep their own simple colliders and scripts.
- **Sprites, decals and textures are assets.** The entity blob, shadow decals and view-cone decal are baked once into asset files (by a builder script or `manage_texture`), not created at startup.
- **Default to the editor's own API**: MCP tool calls for creating objects, adding components, making prefabs, importing assets. They run the real serialisation path, so GUIDs and serialized references stay valid and errors surface on the call that caused them.
- **Escalate to a builder script** when the work is bulk or parametric (roughly: more than a dozen near-identical calls), when it transforms assets that already exist, or when the content should be **rebuildable from a config asset**. The facility greybox from the §4 coordinate tables is the main case: rooms, walls, doors and props are generated from a `FacilityLayout` ScriptableObject, placing kit prefabs from the `AssetRegistry` (primitives for gaps) plus their own box colliders, so a wall is moved by editing one value, not by hand-moving forty pieces. The Final Corridor pipe crack points (§7.6) are another. Builder scripts are Editor-only code (the `Editor` assembly), run through a menu item with `execute_menu_item`.
- **A builder script is an asset.** It gets a row in `docs/ASSET-MANIFEST.md` under Constructed content, and it is **idempotent**: running it twice rebuilds, it does not duplicate. Hand-placed objects (guards, routes, flame volumes) live outside the builder's root so a rebuild never deletes them.
- **Never hand-edit `.unity`, `.prefab` or `.asset` files.** They are a serialisation format, not a source format. Editing them breaks GUID references silently: the scene loads, the component is there, the reference is null, and nothing throws.
- **Search before creating**, and never delete or rename an existing scene, prefab or ScriptableObject without asking. References to them break quietly.
- **Manual setup by the owner is acceptable.** If a step can't be done safely through MCP (or would need a hand-edit), ask the owner to do it, with exact click-by-click instructions.

---

## 5. Art, audio and placeholder assets

The game must never be blocked on art, audio, fonts or VFX. Visuals come from the **approved free packs** in `docs/assets/free-assets.md`, imported by the owner. Unity primitives are used **only for gaps** no approved pack covers. When a needed asset is in neither, use the simplest possible placeholder, then tell the owner.

### 5.1 Approved packs

- **Approved:** every item in `docs/assets/free-assets.md` §1 (the ten store and itch.io packs, plus the two hand-added models: the CC0 low-poly lantern and the CC BY pocket watch), and the sound and sprite sources §2 names for audio and the oil drip (Sonniss GDC bundle, Footsteps – Essentials, Kenney Sci-fi Sounds, Kenney Particle Pack).
- **Not approved until the owner says so:** pack #7 Starter Assets (it pulls in Cinemachine and the Input System, a package change under section 3), and every fallback or alternative §2 and §4 of the asset list mention (Mixamo, Quaternius, Low Poly Soldiers Demo, Modular Sci-Fi Corridor, Human Basic Motions, the stylized smoke pack). Ask before using one.
- **The owner imports; the agent never downloads.** The agent can't sign in to the Asset Store and doesn't fetch files from the internet. If a pack the task needs isn't in the project, stop with a `BLOCKED` message naming the pack and the import steps from `docs/assets/free-assets.md` §3, and continue with other tasks.
- **Check before relying on a pack.** The asset list says several "covers" claims are unconfirmed. Before a task depends on a specific prop or animation, search the imported pack for it. If it isn't there, use the gap fallback the asset list gives for that need, and record the substitution in `docs/ENGINEERING-NOTES.md`.
- **Packs are read-only.** Never move, rename, edit or delete files inside a pack's folder or `Assets/ThirdParty/`; that breaks references and makes re-importing unsafe. To change a pack asset, make a **prefab variant** (or a material copy) under `Assets/_Project/Art/` and change that.
- **Render pipeline is URP** (section 3). Built-in packs (#1, #3, #4, #10) are converted by the owner with the Render Pipeline Converter during import. If pack materials render pink or wrong, stop and ask the owner to run the converter; don't hand-fix shaders.

### 5.2 How packs plug into the game

1. **One registry.** All visuals and sounds are referenced through one `AssetRegistry` ScriptableObject: one slot per purpose (`GuardBody`, `GuardLantern`, `WallSegment`, `ContainmentDoor`, `FireFloor`, `AlertSfx`...). Each slot points at a pack prefab or clip, or at a `PH_` placeholder when there's a gap. Replacing a visual is a one-slot change.
2. **Gameplay and visuals stay separate.** Gameplay prefabs (guard, door, gate, pedestal, `ShadowArea`, `FlameVolume`) own the scripts, triggers and simple colliders. The pack model is a child visual taken from the registry. Never put gameplay scripts on a pack prefab, and never rely on a pack mesh's own colliders for raycasts or triggers.
3. **Coordinates win.** Kit modules are scaled and placed to fit the design doc §4 coordinates, never the other way round. The `FacilityLayout` builder places kit prefabs from the registry where it used to place cubes, and authors its own box colliders for walls and doors.
4. **Animations** come from Human Soldier Animations FREE (#6) through one guard Animator Controller under `Assets/_Project/Art/`. The grenade throw clip is the lantern throw. Animation never drives gameplay position: guard movement stays the deterministic waypoint lerp (section 3), so turn off root motion.
5. **VFX:** Vefects Fire Floor (#9) is the lantern fire disc and pipe-burst fire, scaled from script to match the gameplay radius (`T-FIRE-*`). The particle visual follows the gameplay value; it never decides contact.

### 5.3 Placeholders for gaps

Gaps the asset list confirms: oil pipes, breaker lever handle, lockers, oil drip effect, and the fallbacks for any prop a pack turns out not to have.

**Allowed placeholders**
- **Geometry:** Unity primitives with a material from an approved pack (for example #1's metal for pipes). Cylinders at 2.5 m for pipes, a primitive handle on a #1 panel for the lever, cubes with a #1 panel material for lockers.
- **Decals and textures:** baked into texture assets by a builder script or with `manage_texture` (section 4.1), never generated at runtime. Black blob for the entity, grey quad for shadows, translucent wedge for view cones, white/red outline for targets.
- **Fonts:** whatever default font is already in the project. If none exists, ask (adding TextMeshPro is a package change).
- **Sounds:** a clip from an approved sound source. If none fits, a short tone baked into a `.wav` asset by a builder script. Each sound event gets its own clearly different sound.
- **Particles:** a basic particle system using a Kenney Particle Pack sprite (oil drip: tinted black), or default settings with a flat color.
- **Icons:** a colored square with a one- or two-letter label.

**Rules**
1. Name every placeholder asset `PH_<Purpose>` (for example `PH_OilPipe`, `PH_LeverHandle`) and keep files under `Assets/_Project/Placeholders/`.
2. Reference placeholders only through the `AssetRegistry` (5.2), like pack assets.
3. Never use AI generation, assets from other projects, or anything downloaded that isn't on the approved list.
4. Add a row to `docs/PLACEHOLDERS.md` **in the same commit** as the placeholder:

   | ID | Placeholder | Used by | Final asset needed (format, size, count, style notes) | Status |
   |---|---|---|---|---|
   | PH-001 | `PH_OilPipe` (cylinder, #1 metal material) | `CrackablePipe` (S16, §7.6) | Wall pipe mesh that splits into 3 m crack segments, matches the #1 sci-fi style | Open |

5. At the end of the task, tell the owner which placeholders you added and exactly what's needed to replace each one.

### 5.4 Licenses and credits

- **Every third-party asset used gets a row** in the Third-party section of `docs/ASSET-MANIFEST.md`, in the same commit that first uses it: registry slot, pack name, source link, license.
- **Credits are required.** The pocket watch is CC BY 4.0: the win screen (or a credits screen) must show "Vintage Pocket Watch by Mune (Sketchfab), CC BY 4.0" before the Time SCP stopwatch counts as done. Also credit Vefects for the fire VFX, since its license isn't stated.
- **Keep third-party files out of any public repo.** The Asset Store EULA forbids redistributing the raw files. Before the first commit that adds pack-dependent content, check that `.gitignore` excludes `Assets/ThirdParty/` and each pack's folder, and ask the owner whether the repo is public if you can't tell.

---

## 6. Workflow

- **Phases are the design doc's build steps `B1`–`B10` (§15).** Work on one step at a time, in order. A step is done only when all its tasks meet the definition of done **and the game is playable at that step** (§15: "each step playable before the next").
- Commits reference task IDs: `TASK-B3-004: guard history sampling (S7, §8.2)`.
- **Definition of done for a task:** compiles with a clean console (no errors, no new warnings), all tests pass via `run_tests`, the beats and tunables it delivers have tests or a reported manual check, placeholders and third-party assets are logged (with credits shown where a license requires it), and any affected doc is updated.
- **Jam clock:** the whole build has 12 hours. If a build step is running well past its share, say so to the owner at the end of the current task, with the §15 cut list item that would recover the most time. Don't cut anything yourself.
- Keep the owner informed: at the end of each task, give a short summary of what changed, test results, new placeholders, third-party assets first used, and open questions.

---

## 7. Procedures and skills

The three procedures below are done directly by the agent. No dedicated skill exists for them; the "Must produce" and "Fails if" columns are the checklist.

**The skills `caveman` (terse reply style) and `ponytail` (minimal code) are mandatory.** Invoke `/caveman full` and `/ponytail full` at the start of every session, before any MCP call, and again after any context reset or compaction and at the start of each build step. Never turn either off unless the owner says so for that session. If either is missing, stop with a `BLOCKED` message (section 2) asking the owner to install it; don't imitate it. They govern *how* the agent talks and codes, never *what* this file requires: they don't produce any of the documents below, and they don't shorten them. `SKILLS.md` has the install commands and the full precedence rules (what caveman doesn't compress, and where ponytail's ladder is bounded by sections 3 to 5).

### 7.1 Mandatory procedures

| Procedure | Fires when | Input | Must produce | Fails if |
|---|---|---|---|---|
| **Task list** | Start of every build step (`B1`–`B10`), before writing any code | The step's entry in design doc §15, the systems it names in §14, their mechanic sections, the tunables they use (§13), and any beat acceptance criteria (§9) it delivers | `docs/tasks/B{n}.md`: a stripped task list of nouns and verbs only, each item completable in ≤30 min, each tagged `TASK-B{n}-{nnn}` and with the `S*`, `T-*` and beat ids it serves. Each task that touches Unity ends with its verification step (clean console, named tests passing, or a named manual check in Play mode). The last task is a play-through confirming the step is playable. | Any line contains "consider", "maybe", "as needed" or "if possible"; fewer than 3 items; a line doesn't start with a verb; a task mixes `Logic` code and scene/prefab work; a task depends on an unresolved `OD-*` without saying so |
| **Decision record** | The moment a design decision has ≥2 defensible answers, mid-step, whenever it happens. This always includes every still-open item in design doc §16. | The decision + the constraining design doc sections, `T-*` rows and `AD-*` records | A new `docs/OPEN-DECISIONS.md` row (owner must decide), an ADR in `docs/adr/` (only after the owner decides), or a `docs/ENGINEERING-NOTES.md` paragraph (local and reversible only) | Fewer than 2 rejected alternatives named; a rejection with no reason given; a choice recorded as made when it touches anything section 2 or section 3 says needs the owner |

**Rules that make this non-negotiable:**

1. **Do not skip the task list because you already know the tasks.** What gets checked is the stripped task list in `docs/tasks/B{n}.md`, not your mental model of it. Keep it short: in a jam the list is a checklist, not a document.
2. **A decision record is written the moment the fork appears, not in end-of-step cleanup.** If you notice mid-implementation that you're choosing between two designs, stop and record it there. Don't keep coding and rationalize the choice afterwards. If the decision needs the owner, ask as well (7.2).

### 7.2 On-demand procedure: owner question

Not tied to a build step. It runs **whenever you need information from the owner**: at the start of a step, in the middle of a task, or at the end.

| Procedure | Fires when | Input | Must produce | Fails if |
|---|---|---|---|---|
| **Owner question** | Any time section 2 says to ask: a missing file or asset reference, an ambiguous or conflicting requirement, an unresolved `OD-*` blocking a task, an unverifiable Unity API or version question, a package or settings change, a proposed cut, or a decision record that needs the owner | The gap, what you already checked, and the related `TASK-B*-nnn`, `S*`, `T-*`, beat, `AD-*` and `OD-*` ids | One `BLOCKED` message in the section 2 format, with concrete questions the owner can answer directly, plus the list of tasks you'll continue with meanwhile | The question could have been answered from the design doc (including its `[Default]` items), the codebase, `unity_docs` or `unity_reflect`; the "Looked at" line is empty; fewer than 2 options listed where a choice is being asked for; work continued on the blocked item using a guess |

**Rules:**

1. **Look before you ask.** Check the design doc, this file, the `docs/` folder, the codebase, and the MCP docs tools first. Ask only what those can't answer.
2. **Batch pending questions.** If several questions are open at once, ask them in one pass, ordered by how much work each one blocks. Front-load the questions that block later build steps (render pipeline, input system, Cinemachine/ProBuilder) at the start of the jam.
3. **Record every answer** so it's never asked twice: owner decisions become an ADR in `docs/adr/` (and close the matching `OD-*` row), and facts go into the doc they belong to. An answer that changes the design (a tunable, a default, a beat) is reported to the owner as a needed update to `docs/design/nullflame-design.md`; don't edit the design doc's rules yourself unless the owner says to.
