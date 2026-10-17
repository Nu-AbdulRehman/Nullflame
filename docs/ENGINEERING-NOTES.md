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


### EN-037: The intro gives control after the camera pull, and the skip simulates the pull too (IMPL §21)
`IntroSequence.Run` waits for G-GLANCE to leave D-PLAYER, blends the camera from `Shared/IntroCloseUp` over T-INTRO-CAM, then calls `BeginPlaying`. `Skip` keeps ticking the wave for the same T-INTRO-CAM after G-GLANCE leaves, so retries start with guards exactly where a full intro leaves them (`IntroTests.Skip_MatchesFullRun_GuardPositions`). Rejected: giving control while the camera still blends, which hands the player a moving camera.

### EN-038: The intro close-up pose is a cosmetic scene transform
`Shared/IntroCloseUp` sits 1.8 m west of the entity spawn, 1.1 m up, looking at the entity with D-PLAYER behind it. The design doc gives no camera coordinates; this is a look value, not gameplay, so drag it in the Scene view to reframe. Rejected: computing the pose from the orbit camera, which adds code for a value the owner may want to set by eye.

### EN-039: No containment doors animate in the intro
The Meltdown root holds no door objects: the doorways are open in Meltdown and the closed doors live under `State_Normal`. The intro starts with the doors already open. Rejected: adding Meltdown-only door visuals just to slide them away, which the greybox doesn't need.

### EN-040: Flame vignette uses a second global Volume; hit flash is a HUD image
`PostFx/FlameFxVolume` (profile `Settings/FlameFxProfile.asset`, orange Vignette) has its weight driven by `ScreenFx` from `FlameReceiver.IsInFlame`. `HUD/HitFlash` flashes red on the intro graze and each time the entity enters flame light. Rejected: changing the time-effect profile, which `SetTimeEffect` drives through its own weight.

### EN-041: Clips reach AudioSources through `RegistryAudio` and `SfxPlayer`
In Unity 6, `AudioSource` serialises an `AudioResource`, which MCP can't set. `RegistryAudio` puts the registry clip on its AudioSource in `Awake` (the lantern fire loop, the klaxon), and `SfxPlayer` does the same for 2D one-shots and loops. This also keeps "clips only from the AssetRegistry" (IMPL §22.1). The registry stores sounds as an `(SfxId, clip)` list. An empty slot is silent. `ConsumeLoop`, `PipeCreak`, `OilDrip` and `DoorSlide` have no call yet: they need a hold loop or a door animation the greybox doesn't have.

### EN-042: Hint wording other than the reverse hint is agent-written
§12 names the six hints but gives text only for reverse (§9 Beat 2, copied exactly). The others are short imperative lines built from §3 controls, held in `HintPresenter.texts` so the owner can rewrite them in the Inspector. Each shows for 5 s, once per run.

### EN-043: Guard reverse uses a backward-walk state instead of a negative speed multiplier (IMPL §10.6)
MCP can't set a state's speed parameter, so `Art/Guard.controller` has a `WalkBack` state on pack #6 `HumanM@Walk01_Backward`, entered while `Direction < 0`. `GuardAnimation` sets `Speed` to the measured ground speed (unsigned) and `Direction` to −1 while reversing. The guard keeps facing its route direction while reversed, so a backward walk reads correctly. `Consumed` is reached from Idle and Walk, not Any State, so the bool can't restart the clip every frame. Root motion is off. Rejected: a negative `Animator.speed`, which plays every layer backwards.

### EN-044: `GuardAnimation` finds its guard and Animator with `GetComponent`
Wiring references between objects inside one prefab isn't reliable through MCP (instance ids are temporary), so `GuardAnimation` and `LanternAttack` look up their siblings in `Awake`. Both are on the same `P_Guard` root, so the lookup can't miss. Rejected: unpacking and re-saving `P_Guard`, which would flatten its nested `P_ShadowArea`.

### EN-045: Kit slots filled for walls, floors, doors and props; lights and frames left as primitives (IMPL §23)
`AssetRegistry` now points `floorTile`, `wallSegment`, `containmentDoor`, `showOnlyDoor`, `crate`, `cart`, `cabinet`, `bunk`, `lanternRack` and `labBench` at pack #1 and #3 prefabs; `FacilityBuilder` stretches each to its box. Left empty: `doorFrame` and the #1 animated doors (they carry their own scripts and triggers), `ceilingLight` (the #1 light prefab adds a second Light per fixture), `alarmLight` (needs a red emissive material copy) and `locker` (no pack has one). The guard body is the pack #5 SWAT model under `P_Guard/Visual/Body`, placed in the prefab like the other prefab visuals, not through a registry slot.

### EN-046: Shadow decals are near-black and the scene lights cast real shadows
Play-testing (2026-10-17) showed the grey shadow decals reading as lit boxes on the dark floor. `PH_ShadowDecal` is now near-black (alpha 0.85, wider soft edge) and `ShadowArea` tints a Soft shadow's decal to 55% through a `MaterialPropertyBlock`. Ceiling, alarm, wall-fire and guard-lantern lights now cast soft real-time shadows, so each object also shows its true silhouette on the floor. Both are looks only: gameplay still reads only the `ShadowArea` footprint (design doc §7.1). Rejected: shaping the gameplay footprint to the object silhouette, which would make the walkable area depend on light placement and break the deterministic shadow rules.

### EN-047: Prop, guard and room shadows are drawn by the lights, not by decals
Owner feedback (2026-10-17): the rectangular decals still read as boxes. `ShadowArea.hideDecal` (true on `P_ShadowArea`, so every static shadow, the guard shadow, the pedestal, lever and Final Gate shadows) turns the decal renderer off; the visible shadow is the real-time shadow the scene lights cast from the owner (EN-046). The gameplay footprint is unchanged and still shows as the white/red outline when targeted (§12). Oil drops, void shadows and pipe strips keep their decals: nothing casts them. Known mismatch: the walkable rectangle and the lit silhouette differ where a light sits off-axis; move the light or the `ShadowArea` in the Inspector if a beat reads wrong. Rejected: shaping the footprint from the shadow map, which §7.1 forbids.

### EN-048: Guards hold a lantern; flame light stops at walls; the view-cone decal is off
Owner feedback (2026-10-17): the floor view cone read as fake light that passed through walls. Changes, all gameplay-neutral except the first:
- `FlameReceiver` now line-casts (`LevelSolid`) from a lantern or alarm volume to the entity before applying drain; a wall between them blocks it. Instant-kill fire volumes are untouched (`FlameTests.LanternBehindWall_NoDamage`).
- `LanternHold` on `P_Guard/Lantern` moves the lantern (light, damage sphere, model) to the `Hand_R` bone every `LateUpdate`, so the hand animation carries the light. The damage sphere sways with the hand by a few centimetres; its 3 m radius makes that invisible in play.
- The hand lantern model is the CC0 PolygonDan `polygondan_lantern.obj` at 4% scale, nested under `Lantern`.
- The `ViewCone` decal renderer is disabled in `P_Guard`; the guard's facing and its lantern light show where it looks. Re-enable the renderer in the prefab to get the cone back. Spotting still uses the §8.3 cone and line of sight.
Rejected: a spot light for the lantern (a lantern is omnidirectional; §7.7 says sphere), and clipping the cone decal against walls (needs a depth-tested projector, which is the Decal renderer feature again).

### EN-049: The hand lantern glows with an unlit material
`Art/M_LanternGlow.mat` is URP Unlit, orange, on `P_Guard/Lantern/LanternModel/default`. Unlit reads as self-lit under any light and needs no emission keyword, which MCP can't enable on a URP Lit material. One colour for every guard; G-TIME's amber lantern differs only in its light colour. Rejected: URP Lit with `_EMISSION`, which MCP can't switch on without a hand edit of the material.

### EN-050: Route-versus-obstacle check (2026-10-17)
A read-only editor query walked every `WaypointRoute` segment, every spawn-to-first-waypoint leg and every travel-to-post join against the `Level_Generated/Props` renderer bounds and a `LevelSolid` line cast at 1 m. No route crosses a prop. The only `LevelSolid` crossings are SP-SIDE-N's legs through the Semi-Final Gate, which opens for guards (T-GATE-ZONE 2 m ahead at 2 m/s = 1 s; the panels slide in 0.5 s). The only crossings of the SCP door line are G-TIME's through D-SCP-T, open in Meltdown. Guards do walk through each other: there is no avoidance, by design (deterministic routes, §8.1).

### EN-051: `GuardMotion.Walk` is the one leg-walker for travel and detours
`TickTravel` and the new `TickDetour` share `Walk(pts, ref idx, dt, record)`; the post loop keeps its own modulo loop. A detour clears any dwell left at the waypoint the guard was standing on. Rejected: a second motion class for errands, which would need its own history handling.

### EN-053: `FlameVolume.IsLight` separates light rules from fire rules
Walls and opaque cover stop light (lantern, alarm); they never stop fire. `FlameReceiver` reads `IsLight` for both checks instead of `!InstantKill`, which stopped being a usable proxy once lantern fire became a drain (AD-022).

### EN-052: The SWAT model's tonfa is hidden
`SWAT_A_Tonfa` on the pack #5 body was active and read as a grey object in the lantern hand. Disabled in `P_Guard`.

### EN-054: Pack prefabs are zeroed after `Instantiate(prefab, parent)`
Pack #8's `MediumFlames` and `TinyFlames` prefab roots store their demo-scene positions ((-6.75, -0.04, -21.75) and (-6.5, 0.09, -16)); `Instantiate(prefab, parent)` keeps that as the local offset, so the lantern fire drew about 23 m from its collider and the wall fire about 16 m from its lantern. `LanternFire` and `FacilityBuilder.Instantiate` now reset local position and rotation after instantiating. Pack #10's `Latern1` renders magenta: its materials are Built-in shaders, which is the owner's Render Pipeline Converter step (CLAUDE.md §5.1), not a code fix.
