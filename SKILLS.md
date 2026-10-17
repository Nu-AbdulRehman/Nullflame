# SKILLS.md — Nullflame

The skill registry and skill rules for this project. Read it at session start, right after `CLAUDE.md` (`AGENTS.md` gives the order).

No agent tool loads a file called `SKILLS.md` by itself. The Agent Skills standard is one folder per skill holding a `SKILL.md` (for Claude Code: `.claude/skills/<name>/SKILL.md` in the repo, or an installed plugin). This file is the project's list of which skills are used, when they must be invoked, and how they rank against the project rules. `CLAUDE.md` and `AGENTS.md` point here so it gets read.

---

## 1. Mandatory skills

Both skills are **always on** for every session on this project. They are not optional and not "if installed".

| Skill | What it does | Source | Mode for Nullflame |
|---|---|---|---|
| **caveman** | Terse reply style: no filler, pleasantries, restating or closing offers. Code is unchanged. | `JuliusBrussee/caveman` (MIT) | `full` |
| **ponytail** | Minimal code: reads the code a change touches, then stops at the first rung that works (skip it, reuse, stdlib, native feature, installed dependency, one line, minimum new code). | `DietrichGebert/ponytail` (MIT) | `full` |

### 1.1 Invocation (every session)

1. After reading `CLAUDE.md` and this file, and before any MCP for Unity call, run:
   - `/caveman full`
   - `/ponytail full`

   Other tools use their own skill call (for example `@caveman`, `@ponytail` in Codex).
2. Confirm both are active (each skill acknowledges its mode). Ponytail may already be active through its session hooks; invoke it anyway so the mode is `full`.
3. **Re-invoke both** after any context reset or compaction, at the start of each build step, and whenever replies or code stop following them.
4. Never run `stop caveman`, `stop ponytail`, or switch to another mode unless the owner tells you to for that session. An owner instruction to change mode lasts for that session only.

### 1.2 If a skill is missing

Don't imitate it from memory and don't install it yourself (installing a plugin changes the agent environment, which is the owner's call). Stop and send this `BLOCKED` message, then continue only with reading and planning:

```text
BLOCKED: mandatory skill <caveman|ponytail> is not installed
Task: session start
Looked at: skill list, /caveman or /ponytail returned <error text>
Options I can see: (a) owner installs it with the commands below, then restarts the session; (b) owner waives it for this session in writing
Needed from you: run the install commands, or a written waiver
```

Install commands for Claude Code (send each as its own command):
- caveman: `/plugin marketplace add JuliusBrussee/caveman` then `/plugin install caveman@caveman`
- ponytail: `/plugin marketplace add DietrichGebert/ponytail` then `/plugin install ponytail@ponytail`

If the live install instructions on either repo differ from these, follow the repo and tell the owner this file is out of date.

---

## 2. How the skills rank against project rules

`CLAUDE.md` and the design doc always win. The skills change *how* the agent writes and codes, never *what* the project requires. When a skill's behaviour conflicts with a project rule, follow the rule and keep the skill on.

### 2.1 caveman

| caveman applies to | caveman does **not** compress (write in plain, complete sentences) |
|---|---|
| Chat replies, progress notes, the end-of-task summary, explanations of what changed | Anything written to a file: `docs/**` (task lists, ADRs, open decisions, engineering notes, placeholder rows, asset manifest), code comments, commit messages |
| The "Looked at" and "Needed from you" lines of a `BLOCKED` message | The "Options I can see" line of a `BLOCKED` message: every option keeps its one-line trade-off, readable without context |
| | Click-by-click setup instructions for the owner (`CLAUDE.md` 4.1), including asset import steps |
| | License credit lines (for example the CC BY pocket watch credit): copied exactly, never shortened |
| | Exact error text, console output and test counts: quoted as-is, never paraphrased |

The end-of-task summary is still required in full (changes, test counts, new placeholders, open questions). Caveman makes it short; it doesn't make it optional.

### 2.2 ponytail

Ponytail runs inside the project rules. Its ladder never removes something the project requires.

| ponytail behaviour | Nullflame rule that bounds it |
|---|---|
| "Skip the work if no one needs it" | Never skips: the task list and decision records (`CLAUDE.md` 7.1), tests, placeholder rows, doc updates, the `GameTuning` lookup for numbers, the placeholder registry, `OnDisable`/`OnDestroy` cleanup, or prefab and builder-script discipline (`CLAUDE.md` 4.1). If ponytail says a requirement isn't needed, raise it as an owner question; don't drop it. |
| "Reuse existing code" | For art and sound this means the approved, already-imported packs first (`CLAUDE.md` section 5): a pack prefab before a primitive, a prefab variant before a new model. Never downloads anything and never edits a pack in place. |
| "Use an installed dependency" | Only packages already in the project. Never adds one (`CLAUDE.md` section 3); pack #7 Starter Assets still needs package approval. |
| "Use a native platform feature" | Unity 6 built-ins only, verified with `unity_docs` or `unity_reflect` when you aren't sure they exist. |
| "Write one line" / minimum code | Minimum inside the architecture: rules that don't need the engine still go in `Logic` classes with tests; content still comes from prefabs, not runtime construction. |
| "Leave one runnable check" | That's the floor. The project's test rule (`CLAUDE.md` section 3) is higher and wins. |
| Reply format: code first, at most three lines on what was skipped | Keep it, then add the required end-of-task summary. List what was skipped so the owner can see it. |
| Protected areas (validation, data-loss handling, security, accessibility) | Kept as ponytail says. Add: never strip the view-cone floor decal, the hold ring, or other readability feedback the design doc marks MUST. |
| `ultra` mode (challenges the requirement itself) | Not used. The design doc is fixed for the jam; requirement challenges go to the owner as a `BLOCKED` question. |

**End of every build step:** run `/ponytail-review` on the step's changes. Apply findings that fit the rules above; list any you rejected with the rule that blocked them in the step summary.

---

## 3. Project procedures

The three procedures (task list, decision record, owner question) are defined in `CLAUDE.md` section 7 and are run directly by the agent. They are **not** packaged as skills. Caveman and ponytail don't produce their documents and don't shorten their "Must produce" requirements.

---

## 4. Adding or changing a skill

Adding, removing or replacing any skill needs the owner's approval, the same as a package. Other installed skills may be used only where they don't conflict with `CLAUDE.md`, and they never replace caveman or ponytail.

If the owner approves a new project skill, write it to the standard:

1. Location: `.claude/skills/<name>/SKILL.md`, committed to the repo.
2. Frontmatter: `name` (lowercase, digits and hyphens, ≤64 characters, same as the folder name) and `description`.
3. Description: third person, says what the skill does **and** when to use it, with the trigger words an agent would see (for example "Use when starting a Nullflame build step").
4. Body: numbered steps and checklists, not prose. Under about 500 lines.
5. Detail goes in `references/` (docs read on demand), `scripts/` (helpers to run rather than rewrite) or `assets/`, linked one level deep from `SKILL.md`.
6. No secrets, keys or tokens in any skill file.
7. Test it in a fresh session by asking for the workflow without naming the skill. If it doesn't load, sharpen the description's trigger words.
8. Add a row to the table in section 1 (if mandatory) or section 5, in the same commit.

---

## 5. Other skills in use

None. Add a row here when the owner approves one.

| Skill | Source | When to invoke | Approved |
|---|---|---|---|
| – | – | – | – |
