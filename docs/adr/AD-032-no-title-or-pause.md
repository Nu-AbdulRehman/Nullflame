# AD-032: No title screen and no pause menu in the Facility scene

**Date:** 2026-10-18 · **Overrides:** design doc §3 (Esc = pause menu), §12 (Title screen row, Win screen "Return to title"), §14 S17 · **Decided by:** owner ("We don't need the title scene, we'll be replacing it with a menu scene. We also do not need a pause menu either. Remove these 2 for now.").

## Decision
- `UI/Canvas/TitleScreen` and `UI/Canvas/PauseScreen` are deleted from the Facility scene. A separate Menu scene will take the title's place later.
- `GameFlow` loses `Play`, `Pause`, `Resume` and the `Title`/`Paused` flow states. On load, `HasSeenIntro` false runs the intro at once (`FlowState.Intro`); true skips it as before. Esc does nothing; `InputReader.PausePressed` is gone.
- The win screen's button is "New game" (`GameFlow.NewGame`, was `ReturnToTitle`): it clears `HasSeenIntro` and reloads, so the intro plays again. `MenuAction` keeps its old enum values so the saved death and win buttons stay bound.
- Tests that need the spawns untouched call `IntroSequence.Cancel()` before `BeginPlaying()` (EN-065).

## Consequences
Design doc §3, §12 and §14 need the owner's edit. OD-016 (Space skip) now reads "after Win → New game". When the Menu scene arrives it should load Facility with `HasSeenIntro` false for a new game; `GameFlow.Quit` stays for it. Rejected: keeping an auto-hidden title behind a flag (dead UI in the scene), and keeping Esc as a bare time-freeze without a menu (no way to resume by mouse; the owner asked for no pause).
