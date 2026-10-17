# Engineering notes

Small, local, reversible choices and why they were made. Anything that touches what `CLAUDE.md` sections 2 or 3 reserve for the owner goes to `docs/OPEN-DECISIONS.md` instead.

`IMPL §n` refers to `docs/implementation/nullflame-implementation.md`.

### EN-001: Floor decals are unlit quads, not URP Decal Projectors (IMPL §2.8)
Shadows, the entity blob, view cones, oil drops and target outlines are flat quads with unlit transparent materials, layered by height (0.010 to 0.020 m) and render queue. Rejected: URP Decal Projectors, because they need the Decal renderer feature added to the URP renderer asset, which `CLAUDE.md` §3 reserves for the owner. Rejected: projected shadows from real lights, because gameplay must read only `ShadowArea` components.

### EN-002: Route timing and coordinates are level data, not tunables (IMPL §3.3)
Waypoint positions, per-waypoint dwell times and look-at targets live on scene objects; prop and pipe positions live in `FacilityLayout`. Rejected: one `GameTuning` row per waypoint, because it would put dozens of coordinates in the tuning asset and disconnect them from the gizmos the owner drags. Rejected: hard-coding routes, because `CLAUDE.md` §4.1 requires routes as scene objects.

### EN-003: Guard history also records each waypoint as it is reached (IMPL §4.2)
Sampling every 0.25 s (§8.2, C5) cuts corners by up to about 0.3 m at 2.4 m/s. Adding each waypoint position to history when the guard reaches it makes the recorded polyline contain every corner, so reverse playback retraces the exact path (`CLAUDE.md` §3). Rejected: per-frame sampling (C5 replaced it). Rejected: reversing the waypoint list instead of history, because history must also capture interrupted travel.

### EN-004: A despawning ridden guard leaves a void shadow (IMPL §7.3)
§5.2 covers a shadow destroyed by a consume or an expiring oil drop. A spawned transit guard that despawns at a show-only door while ridden is the same case, handled by the same `Removed` event. Rejected: blocking despawn while ridden, which needs extra state and would leave a guard frozen at a door.

### EN-005: Target highlight is a LineRenderer loop of the footprint (IMPL §7.5)
Four corners at Y = 0.018, white or red. Rejected: an outline shader or a second scaled quad per shadow, both more assets for the same readability.

### EN-006: The Meltdown lever handle rotates with rewind-channel progress (IMPL §14)
§9 Beat 3 says the lever is shown moving down. The rewind effect hides the state swap, so the handle animates during the 2 s channel instead. Rejected: an animation after the swap, which the white-out would hide.

### EN-007: "In transit" spawner caps ignore guards on Hold (IMPL §17)
§8.4 caps SP-SIDE-N at "2 in transit". A guard the player has reversed and left on Hold stays alive forever; counting it would halve or stop the flow Beat 4 depends on. Other caps ("max alive") count every alive guard. Rejected: counting all guards everywhere, which can stall Beat 4.

### EN-008: Return to title resets `GameFlow.HasSeenIntro` (IMPL §19)
The title screen shows when `HasSeenIntro` is false. Win → Return to title sets it false and reloads, so the title and intro play again for a new game. This keeps `HasSeenIntro` as the only static that survives a reload (`CLAUDE.md` §3). Rejected: a separate Title scene (needs another build-list change), and a second static flag (breaks the one-static rule).

### EN-009: Win-screen time counts from the moment control is given (IMPL §19)
"Time taken" (§12) starts at the end of the intro or the intro skip, and resets on death because the scene reloads. Rejected: counting from the Play button, which would include the unskippable first intro.

### EN-010: UI text uses uGUI `Text` with the built-in `LegacyRuntime.ttf` (IMPL §20)
`CLAUDE.md` §5.3 says to use the default font already in the project. TMP ships inside uGUI 2.0 but needs its Essential Resources imported into `Assets/`. Switching later is a per-label change. Rejected: TMP now, which adds imported assets the owner hasn't asked for.

### EN-011: The intro skip fast-forwards guard motion instead of teleporting guards (IMPL §21)
Retries start at Beat 1 with the wave about to pass D-PLAYER. Ticking each wave guard's deterministic `GuardMotion` in 1/60 s steps up to the control moment produces the same positions and histories as a full intro, so Beat 3's reverse works the same on a retry. Rejected: hand-placed retry start positions, which would leave G-TIME with a short history and break Beat 3.

### EN-012: Lab benches come from pack #1, not pack #4
Checked on 2026-10-17: Free Laboratory Pack (#4) contains only flasks and pipettes (`Assets/FreeLabAssets/Prefabs`), no benches. The `LabBench` registry slot uses #1's `Decorative elements/Tables/desk_no_computer.prefab`, the fallback `docs/assets/free-assets.md` §2 lists. #4's flasks can dress the desks. Rejected: a primitive bench, since #1 covers the need.

### EN-013: The entity owns void-shadow cleanup; no VoidShadow component (IMPL §7.3, §13.3)
`ShadowEntity` remembers the void it spawned and destroys it when it attaches to any other shadow. Rejected: a separate `VoidShadow` component polling the entity every frame, which is a second script for one rule. The `Removed` handlers only set flags; the void spawns on the next `Update`, because `ShadowArea.OnDisable` also runs during scene unload.

### EN-014: `ShadowArea.isMoving` is a serialized flag (IMPL §7.1)
The guard prefab sets it. Rejected: computing it from a `GuardController` in the parents, which couples the shadow script to the guard script before guards exist and adds a lookup.

### EN-015: Fire visuals use the Particle Pack (#8), not Vefects (#9)
Every Vefects fire material uses `SH_Vefects_VFX_URP_Fire_Flames_01`, which fails to compile on URP 17.3 ("redefinition of '_CameraDepthTexture_TexelSize'", d3d11). The pack is read-only, so `lanternFireVfx` and `fireSmallVfx` point at #8 `MediumFlames` and `TinyFlames`, the fallback `docs/assets/free-assets.md` §2 lists. Rejected: editing the Vefects shader (packs are read-only, `CLAUDE.md` §5.1). The approved Vefects credit (AD-015) is only needed if a Vefects asset is used later.

### EN-016: Alarm lights and wall fire lanterns are built by FacilityBuilder (IMPL §11.3)
Their positions are layout data (Appendix A.3), so they live in `FacilityLayout` and the builder places them under `State_Meltdown/Generated_Meltdown`, with visuals from `AssetRegistry` slots. This keeps them visible and editable in the scene and rebuildable. Rejected: hand-placed prefab instances (positions duplicated outside the layout) and runtime instantiation (invisible in Edit mode).

### EN-017: Damage collider sizes come from GameTuning at Awake (IMPL §11.1)
`FlameVolume` sets the lantern sphere radius to T-LANTERN-RADIUS and the alarm capsule radius to T-ALARM-RADIUS, and `LanternFire` sets its sphere from `FireRadius`. Rejected: typed collider sizes in prefabs, which would drift from the tunables.

### EN-018: Lanternless guards keep a white cone (IMPL §10.5)
The spec asks for a grey-tinted cone while lanternless. Skipped for now: it needs a per-guard material instance, and the hidden lantern already shows the state. Add in B10 polish if play-testing shows confusion.

### EN-019: Consume effects are chosen by kind in code, not a UnityEvent (IMPL §13.1)
`ConsumeAction` grants the time power when a `Pedestal` is eaten (and will call the win in B9 for the `FinalGate`). Rejected: an Inspector-wired `onConsumed` UnityEvent, which MCP for Unity can't wire reliably and which adds a field for two fixed outcomes.

### EN-020: After a cancelled consume, E must be released before a new one starts
Without the latch, a consume cancelled by spotting restarts on the next frame while E is still held. The latch also applies to release-cancels, where E is up anyway.

### EN-021: `InputReader.Simulate` drives holds in tests
PlayMode tests call `Simulate(consume, reverse, rewind)`, which stops device reads for that reader. Rejected: test-only public setters on each gameplay component, which spreads the hook.

### EN-022: Scanlines are a hand-written HLSL shader driven by a global, not a Shader Graph (AD-013)
`Assets/_Project/Shaders/Scanlines.shader` uses URP's Blit.hlsl, which is what the Full Screen Pass Renderer Feature expects. `ScreenFx` sets the global `_TimeFxIntensity`; at 0 the pass leaves the image unchanged, so the feature stays enabled rather than being toggled. Rejected: a Shader Graph (a large JSON asset that can't be authored safely through MCP), toggling the feature with `SetActive` (needs a reference to a renderer sub-asset), and setting a property on the material asset at runtime (it would change the asset in the Editor). The desaturation is a Volume (`PostFx/TimeFxVolume`, Color Adjustments saturation −100) whose weight follows the same intensity. Main Camera now has post-processing enabled.

### EN-023: A guard that finished its route can be reversed
`GuardMotion.BeginReverse` used to refuse the Idle state, so a guard that reached the end of a route with no post loop (Beat 4 transit guards) couldn't be reversed. Only an empty history now blocks a reverse.

### EN-024: A rewind channel needs Q released after it ends, and dies outside Meltdown
Found by `Rewind_IsOneWay`: holding Q after a completed rewind started a second channel during the 0.75 s before the swap, which then ran on into Normal and charged T-COST-REWIND again. The channel now cancels when the world leaves Meltdown, and any ended channel (completed or cancelled) needs Q released first. Same pattern as the consume latch (EN-020).

### EN-025: State_Normal is activated briefly in Edit mode to author its contents
MCP for Unity can't edit objects under an inactive parent, so `State_Normal` is switched on while its routes and guards are authored, then switched off again before saving. `WorldStateController` relies on it starting inactive.

### EN-026: Menu buttons pick a GameFlow action from an enum
`MenuButton` sits next to a uGUI `Button` and calls `GameFlow.Play`, `Resume`, `Restart`, `ReturnToTitle` or `Quit`, chosen by an enum in the Inspector. Rejected: wiring `Button.onClick` persistent listeners, which MCP for Unity can't set reliably (same reason as EN-019). The buttons share the `Prefabs/UI/P_MenuButton` prefab.

### EN-027: Restart reloads the scene by name, not build index
`SceneManager.LoadScene(gameObject.scene.name)`. PlayMode tests load the Facility through `EditorSceneManager.LoadSceneInPlayMode`, which gives the scene build index −1, so a reload by index failed there.

### EN-028: The intro skip finds the control moment by simulating, not by a stored time
`IntroSequence.Skip` sets Essence to 90 and steps the wave in 1/60 s ticks until G-GLANCE has paused at D-PLAYER and walked on south. The control moment then follows route edits automatically. Rejected: a tuned "skip to t = N s" number, which would drift whenever a route changes.

### EN-029: Spawners can stagger their first guards
`GuardSpawner.initialSpacing` spaces the guards a refill-only spawner places at the start (`SpawnTimer`), so they don't stand on top of each other at the first waypoint. 0 keeps the old behaviour.

### EN-030: The Beat 1 soft fail ends once the wave guards are gone
`GameFlow.CheckMissedChance` stops checking when any wave guard is destroyed, which happens when the rewind removes `State_Meltdown`. Without this, the destroyed guards counted as "at their posts" and Normal state showed the Missed screen.

### EN-031: PlayMode tests start from the control moment
`TestScenes.LoadFacility` clears `HasSeenIntro`, loads the scene, then calls `GameFlow.BeginPlaying()`, so tests skip the title screen without running the intro skip. Tests that need the wave call `IntroSequence.Run()` themselves. Test waits use game time, because an unfocused Editor runs at a low frame rate and real-time waits under-ran. The Player's Start Shadow must be `PlayerRoomFloor`; a manual check that changes it and saves the scene breaks the beat tests.

### EN-032: The pipe hold ring shows its zone by colour, not three background arcs
`HoldRingView` tints the fill grey, green or red as the meter passes T-PIPE-SAFE-START and T-PIPE-SAFE-END. The spec's three fixed background arcs need extra Image objects and a second sprite; the colour change tells the player the same thing. Add the arcs in B10 polish if play shows players can't time the release.

### EN-033: The pipe hold lives in ConsumeAction; CrackablePipe only acts on the result
`ConsumeAction` already owns E, the release latch and the cost preview, so it runs the `PipeCrackMeter` and calls `CrackablePipe.Crack()` or `Burst()`. Leaving the strip mid-hold drops it with no effect and no cost. Rejected: a second E reader on the pipe (two components fighting over one key) and a shared hold interface (one extra type for two callers).

### EN-034: A burst throw skips the alert
`LanternAttack.ThrowAt` throws at once, ignoring guards already lanternless. The escape window is T-THROW-TIME plus the fire's grow time, as §7.6 describes. The burst pours no visible oil yet; the larger fire (T-FIRE-RADIUS-OIL) is the only effect until B10 VFX.

### EN-035: The Final Gate win is called in code, like the pedestal grant (EN-019)
`ConsumeAction.Complete` calls `GameFlow.Win()` for `ConsumableKind.FinalGate`, so `P_FinalGate` has no `onConsumed` event. The win screen shows "Time taken: m:ss" from `GameFlow.RunTime` and both credit lines word for word (AD-015, CLAUDE.md §5.4).

### EN-036: The Kenney Particle Pack is not imported, so the oil drip uses a flat-colour particle
CLAUDE.md §5.3 allows "default settings with a flat color" when the sprite isn't available. `PH_OilDrip` is a default Particle System with a black URP Particles/Unlit material (PH-015). Import the Kenney Particle Pack to swap in its sprite.

