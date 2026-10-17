# Nullflame: Game Design Document

**Version:** 1.0 (2026-10-17) · **Source:** ideation docs `nullflame/00` to `08`, the facility schematic `SCP_Containment_Layout.png`, and Haider's corrections in the ideation thread (latest wins).
**Engine:** Unity 6 · **Platform:** Desktop (Windows/macOS), keyboard and mouse · **View:** third person 3D
**Build constraint:** 12-hour game jam, implemented by a Claude coding agent. Every system below is specified at the cheapest version that delivers the design.

How to read this document:
- **MUST** = required for the jam build. **SHOULD** = build if on schedule. **CUT** = first to drop (see section 15).
- Every number is a tunable with an ID like `T-ESS-MAX`. All tunables are listed in section 13 and should live in one ScriptableObject (`GameTuning`) so they can be changed without code edits.
- Where the ideation docs left a gap or contradicted each other, this document picks a **default** and marks it `[Default]`. Section 16 lists them so Haider can overrule any of them.

---

## 1. Game summary

You are a newly discovered SCP: a living patch of nothingness that can only exist inside shadows. A facility meltdown pops every containment door open. You ride guards' shadows, steal the Time SCP's power, rewind the facility to before the meltdown, and finally consume the exit gate.

| Theme | Gameplay role |
|---|---|
| **Shadow** | The only place you can exist. You move by jumping between shadows and riding guards' moving shadows. You grow by consuming things through their shadows. |
| **Flame** | The threat. Flame light (guard lanterns, alarm lights, Final Corridor wall fire lanterns) damages or kills you, even inside a shadow. Normal light is harmless. |
| **Time** | The stolen power. After eating the Time SCP you can reverse a guard's movement and rewind the whole facility from Meltdown to Normal. |

**Win:** consume the Final Gate. **Lose:** the Essence bar reaches 0. On death, restart the whole game from the start (no checkpoints, confirmed by Haider). The intro is skippable on retries.

**Length target:** 10 to 15 minutes for a first clear. One continuous level, five puzzles.

---

## 2. Core rules (the contract every system obeys)

1. **The entity is always in a shadow.** It is never on open floor. It is either *in a shadow* (static or moving), *jumping* between shadows, *consuming*, or *dead*.
2. **One bar, Essence,** is both health and energy. Damage drains it, consuming objects and using time powers spend it, consuming guards refills it. At 0 the entity dies.
3. **Opaque shadows hide you; soft shadows don't.** Guards can see you in a soft shadow. In an opaque shadow they can only see you while you are jumping or consuming.
4. **Flame light reaches into shadows.** Guard lanterns drain you while their light touches your shadow. Wall fire lanterns kill instantly.
5. **Being spotted brings fire.** A guard who spots you throws their lantern at you. It smashes into a spreading fire that kills on contact, but you survive if you get out of its reach in time (section 7.9). This is the stealth fail state.
6. **Static cover is for hiding, guards are for travel.** No room has enough static shadows to chain across it. Crossing a room means riding a guard.

---

## 3. Controls

Confirmed by Haider: aim with the camera, a center reticle picks the target, left-click jumps.

| Input | Action | When |
|---|---|---|
| Mouse | Orbit camera | Always |
| W/A/S/D | Slide within the current shadow (clamped to its bounds) | In a shadow larger than the entity |
| Left mouse | Jump to the highlighted shadow | A valid target is highlighted |
| E (hold) | Consume the owner of the current shadow | In a consumable shadow |
| E (hold, on a pipe) | Crack the pipe; release in the safe zone (section 7.6) | In a pipe's shadow |
| R (hold) | Local reverse on the guard you are riding | Has time power, riding a guard |
| Q (hold) | Facility rewind | Has time power, in the lever's shadow, Meltdown state |
| Esc | Pause menu (Resume, Restart, Quit) | Always |

Hold actions show a radial progress ring and the Essence cost before you commit. Releasing early cancels with no cost (except pipe cracking, section 7.6).

---

## 4. Facility layout

### 4.1 Coordinates

Built directly from the schematic at **1 px = 0.05 m**. World axes: **+X = east (right on the schematic), +Z = north (up on the schematic)**, Y up. Origin = bottom-left corner of the Time SCP Room. Floor at Y = 0, ceiling at Y = 4 m (`T-LVL-CEIL`). Haider confirmed this scale.

| Area | X min to max (m) | Z min to max (m) | Size (m) | Player access |
|---|---|---|---|---|
| Time SCP Room | 0 to 7.5 | 0 to 6 | 7.5 × 6 | Yes |
| Other SCP Room A | 7.5 to 15 | 0 to 6 | 7.5 × 6 | No (visible only) |
| Other SCP Room B | 15 to 22.5 | 0 to 6 | 7.5 × 6 | No |
| Other SCP Room C | 22.5 to 30 | 0 to 6 | 7.5 × 6 | No |
| Other SCP Room D | 30 to 37.5 | 0 to 6 | 7.5 × 6 | No |
| SCP Rooms Access Corridor | 0 to 37.5 | 6 to 12 | 37.5 × 6 | Yes |
| Main Corridor | 15.75 to 21.75 | 12 to 40 | 6 × 28 | Yes |
| Player Starting Room | 8.25 to 15.75 | 22.25 to 29.75 | 7.5 × 7.5 | Yes |
| Lab | 21.75 to 32 | 19.5 to 29.75 | 10.25 × 10.25 | Yes, via Barracks only |
| Barracks | 21.75 to 32 | 29.75 to 40 | 10.25 × 10.25 | Yes |
| Sleeping Quarters door (show-only) | 32 to 33.75 | 34.85 to 40 | alcove | No |
| Semi-Final Gate | 15.75 to 21.75 | 40 to 41.5 | 6 × 1.5 | Puzzle 4 |
| Final Corridor | 15.75 to 21.75 | 41.5 to 56 | 6 × 14.5 | Yes |
| West side corridor (show-only) | 9.75 to 15.75 | 53 to 56 | 6 × 3 | No |
| East side corridor (show-only) | 21.75 to 27.75 | 53 to 56 | 6 × 3 | No |
| Final Gate | 15.75 to 21.75 | 56 to 57.5 | 6 × 1.5 | Puzzle 5 |

### 4.2 Doors and openings

A door exists wherever a room meets a corridor, with the Lab as the one exception. Corridor-to-corridor junctions are open (no door).

| ID | Connects | Position (wall, centre) | Type |
|---|---|---|---|
| D-PLAYER | Player Room ↔ Main Corridor | X = 15.75, Z = 26 | Open frame (meltdown popped it) |
| D-BARRACKS | Barracks ↔ Main Corridor | X = 21.75, Z = 35 | Normal door, open in both states |
| D-LAB | Lab ↔ Barracks | Z = 29.75, X = 27 | Normal door, open in both states |
| D-SLEEP | Barracks ↔ Sleeping Quarters | X = 32, Z = 37.4 | Show-only. Guards spawn/despawn here. Player blocked. |
| D-SCP-T | Time SCP Room ↔ Access Corridor | Z = 6, X = 3.75 | Containment door. Open in Meltdown, closed in Normal. |
| D-SCP-A..D | Other SCP rooms ↔ Access Corridor | Z = 6, X = 11.25 / 18.75 / 26.25 / 33.75 | Containment door. Open in Meltdown, closed in Normal. A player-blocking invisible wall sits in each doorway. |
| G-SEMI | Main Corridor ↔ Final Corridor | Z = 40 to 41.5 | **Guard-only gate** (section 7.8) |
| D-SIDE-W / D-SIDE-E | Side corridors' far ends | X = 9.75 / X = 27.75, Z = 54.5 | Show-only. Guards spawn/despawn here. |
| G-FINAL | Final Corridor ↔ outside | Z = 56 | Consumable exit (puzzle 5) |

The Lab has **no** door to the Main Corridor (solid wall at X = 21.75, Z 19.5 to 29.75).

### 4.3 Set dressing and shadow casters per area

Static cover follows rule 6: enough to hide in, never enough to cross a room. Each listed object casts one static opaque shadow unless noted.

| Area | Static shadows | Lights |
|---|---|---|
| Player Room | Whole floor is one large opaque shadow (the room is pitch black) | None. A dim rim light so the player can read the space. |
| Main Corridor | 2 equipment crates (west wall, Z ≈ 16 and Z ≈ 33) | Normal ceiling lights. Meltdown: 2 alarm lights (section 7.7). |
| Access Corridor | 1 cart near the Time Room door (X ≈ 6, Z ≈ 8) | Normal ceiling lights. Meltdown: 2 alarm lights. |
| Time SCP Room | Pedestal (centre, 3.75, 3), 1 cabinet (north-west corner, near the door) | Normal light |
| Barracks | Lantern rack (north wall, X ≈ 29, Z ≈ 39.5), **breaker lever panel** (north wall, X ≈ 24, Z ≈ 39.5, about 5 m from the rack), 2 bunks/lockers (south half) | Spare lanterns on the rack are **unlit** props |
| Lab (SHOULD) | 2 lab benches | Normal light |
| Final Corridor | **Nothing but pipes.** Pipes run along both walls at 2.5 m height and cast **soft** shadow strips on the floor. | Normal ceiling lights (harmless). Wall fire lanterns, 3 per side (Meltdown only, kill volumes). |
| Final Gate | The gate casts an opaque shadow strip in front of it (Z 54.5 to 56) | None |

---

## 5. The player entity

### 5.1 Visual states

| State | Visual |
|---|---|
| In a shadow | A pure black, slightly animated decal on the floor (1.2 m blob, `T-PLY-SIZE`), drawn on top of the grey shadow it occupies. Ordinary shadows are grey, so the player always reads where they are. |
| Jumping | A black teardrop mesh arcing through the air from source to target. |
| Consuming | The black decal grows to cover the target's grey shadow, then the target sinks into it. |
| Dead | The black decal is burned away by fire (or drained to nothing), fade to the death screen. |

### 5.2 State machine

```
            +-----------------------------+
            v                             |
  [InShadow] --LMB on target--> [Jumping] -+  (lands in target shadow)
     |   ^                          |
     |   |                          +--seen mid-jump--> lantern thrown at you (keep playing, escape the fire)
     |   +--consume done/cancel--+
     +--hold E----------------> [Consuming] --seen--> consume cancelled, lantern thrown at you
     +--Essence = 0 / kill volume / touched by fire --------------> [Dead]
```

- **InShadow** has a reference to the current `ShadowArea`. If that shadow is attached to a moving guard, the entity moves with it. This is "riding". There is no separate riding state.
- While riding, W/A/S/D is disabled `[Default]`; the entity sits at the shadow's centre.
- If the current shadow is destroyed (its owner is consumed by the player, or an oil drop expires), the entity stays at that spot inside a small "void" shadow it leaves behind (section 7.5).

### 5.3 Shadow jump

- Valid targets: any `ShadowArea` whose nearest point is within `T-JMP-RANGE` (6 m) of the entity, with no wall between them (raycast at 0.5 m height).
- Target highlight: the shadow under the reticle (or nearest to the reticle ray within 2 m) gets an outline. Out-of-range shadows under the reticle show a red outline.
- Flight: parabolic arc, apex height `T-JMP-ARC` (1.5 m), duration `T-JMP-TIME` (0.45 s). If the target is moving, the arc homes on the target's current position each frame.
- Jumping costs no Essence.
- The jump is **visible**: if any point of the flight is inside a guard's view cone (section 8.3), the player is spotted.

---

## 6. Essence (the one bar)

| Property | Value |
|---|---|
| Max | `T-ESS-MAX` = 100 |
| Start | `T-ESS-START` = 100 |
| Regeneration | None `[Default]` |

| Change | Amount | Tunable |
|---|---|---|
| Intro lantern graze | −10 (scripted) | `T-ESS-INTRO` |
| Guard lantern light touching your shadow | −8 per second | `T-DMG-LANTERN` |
| Alarm light touching your shadow | −5 per second | `T-DMG-ALARM` |
| Wall fire lantern | set to 0 (instant death) | – |
| Lantern fire | set to 0 if the fire touches your shadow (escapable, section 7.9) | – |
| Crack a pipe | −5 | `T-COST-PIPE` |
| Consume pedestal + Time SCP | −20 | `T-COST-PEDESTAL` |
| Consume Final Gate | −80 (confirmed by Haider) | `T-COST-GATE` |
| Local reverse | −5 per second | `T-COST-REVERSE` |
| Facility rewind | −30 flat | `T-COST-REWIND` |
| Consume a guard | +25 (clamped to max) | `T-GAIN-GUARD` |

Spending rules `[Default]`:
- **Object consumes and the facility rewind require Essence > cost.** If you can't afford it, the prompt shows the cost in red and E/Q does nothing. Spending cannot kill you.
- **Held time powers stop automatically** when Essence reaches `T-ESS-FLOOR` (10), with a warning pulse on the bar. Only damage can take Essence to 0.
- Below 25 Essence the bar pulses and a heartbeat-like audio cue plays.

Budget check for the intended route (with defaults): start 100, intro −10, a few seconds of lantern light −5, pedestal −20 → about 65 when puzzle 3 begins. The reverse from the Time Room to the Barracks is about 14 s (section 9) = −70, so the player runs dry once and must eat one guard mid-route, as Haider described. The Final Gate (−80) needs 3 to 4 guards eaten in the Final Corridor.

---

## 7. Mechanics

### 7.1 Shadows (`ShadowArea`)

Shadows are **gameplay objects, not lighting.** Each is a flat grey decal/quad at floor level plus a trigger collider; gameplay reads only these. Real-time shadow maps are not used for gameplay.

| Field | Meaning |
|---|---|
| `owner` | The GameObject casting it (guard, pedestal, pipe, oil drop, gate, crate...). Null for the Player Room floor. |
| `opacity` | `Opaque` (hides you) or `Soft` (guards can see you in it) |
| `isMoving` | True if parented to a guard |
| `bounds` | The floor footprint; W/A/S/D sliding is clamped to it |
| `consumable` | Optional reference to a `Consumable` on the owner |

Shadow catalogue:

| Shadow | Opacity | Moving | Consumable |
|---|---|---|---|
| Player Room floor | Opaque | No | No |
| Guard | Opaque | Yes | Yes (gain) |
| Crate, cart, cabinet, bunk, bench, lantern rack | Opaque | No | No `[Default]`, to keep cover in place |
| Pedestal (with Time SCP on it) | Opaque | No | Yes (cost, grants time power) |
| Breaker lever panel | Opaque | No | No. It is the facility rewind spot. |
| Pipe | **Soft** | No | Partial only (crack, section 7.6) |
| Oil drop | Opaque | No | No |
| Final Gate | Opaque | No | Yes (cost, wins the game) |
| Void (left after a consume) | Opaque | No | No. Small (1.5 m), disappears when you leave it. |

A guard's shadow is a decal offset 0.6 m behind the guard (the side away from their lantern), sized 1.5 × 1 m.

### 7.2 Riding guards

- Being in a guard's shadow means you move with it. This is the main way to cross rooms.
- **A guard's own lantern never damages the shadow it casts** `[Default]` (otherwise riding would always drain you; see section 16). Other guards' lanterns and alarm lights still do.
- The ridden guard does not notice you. Other guards can only spot you through jumping or consuming (guard shadows are opaque).

### 7.3 Consume

1. Player holds E in a consumable shadow. Prompt shows `Consume <name> (cost X)` or `(gain X)`.
2. The black decal spreads over the grey shadow over the consume duration.
3. For guards: the guard stops walking, plays a struggle animation (or just stops), and sinks into the floor in the last 40% of the duration. For objects: the object sinks.
4. On completion: apply Essence change, fire the `OnConsumed` event (grant power, win, etc.), destroy the owner, leave a void shadow.
5. **Detection:** if the entity is inside any other guard's view cone at any moment during the consume, the player is spotted: the consume is cancelled at no cost and that guard throws its lantern (section 7.9). The guard being consumed does not count.
6. Releasing E early cancels with no cost and no effect; the decal shrinks back.

| Target | Duration | Tunable |
|---|---|---|
| Guard | 2.0 s | `T-CON-GUARD` |
| Pedestal + Time SCP | 3.0 s | `T-CON-PEDESTAL` |
| Final Gate | 5.0 s | `T-CON-GATE` |

The pedestal and the Time SCP stopwatch are **one consume** `[Default]`: eating the pedestal also eats the stopwatch on it.

### 7.4 Time powers

Unlocked together when the Time SCP is consumed (`hasTimePower = true`). Until then R/Q do nothing and are hidden from the HUD.

**Local reverse (MUST).** Hold R while riding a guard. The guard walks backwards along its recorded path at `T-REV-SPEED` (2× walk speed), carrying you. Costs `T-COST-REVERSE` per second.
- When R is released, or the history runs out, or Essence hits the floor, the guard enters **Hold**: it stands still at that point, facing the direction it was originally walking, view cone active. It stays on Hold until reversed again or the game restarts. `[Default]`: this lets the player hop off to eat a guard and come back, as Haider described for puzzle 3.
- Doors and the Semi-Final Gate react to a reversing guard exactly as to a walking one.

**Speed up: CUT** (confirmed by Haider). No puzzle requires it.

**Facility rewind (MUST).** Hold Q for `T-REWIND-CHANNEL` (2 s) while in the breaker lever's shadow in the Meltdown state. Costs `T-COST-REWIND`. Being seen during the channel counts as being seen consuming. On completion the world switches from Meltdown to Normal (section 10) behind a full-screen rewind effect. One-way: it cannot be undone.

**Stop time: CUT** (confirmed by Haider). Not used by any puzzle.

While any time power is active: desaturate and add scanlines to the screen, play a reverse "whoosh" loop.

### 7.5 Void shadow

After consuming something, the entity is left in a 1.5 m opaque void shadow at that spot. It is a normal static shadow you can jump from. It despawns when you leave it. This prevents the "shadow disappeared under me" case.

### 7.6 Oil pipes (partial consume)

- Final Corridor pipes are split into crack points every 3 m along each wall (about 4 per wall, 8 total). Each crack point owns a soft shadow segment.
- Hold E while in a pipe segment's shadow. A meter fills over `T-PIPE-BURST` (1.5 s):
  - 0 to 0.6 s: grey (nothing happens if released).
  - 0.6 to 1.1 s: green **safe zone**. Release here = **crack**: −5 Essence, the pipe drips, and over 2 s it spawns **3 oil drops** on the floor below and slightly into the corridor (each a 1.2 m opaque shadow, spaced about 1.5 m apart). Drops are permanent `[Default]`.
  - 1.1 to 1.5 s: red warning zone with a creak sound. Release here = crack as above.
  - Reaching 1.5 s = **burst**: the pipe pours oil onto the floor below it, and every guard in the Final Corridor that has a lantern throws it at the burst point `[Default]`. The spilled oil makes this fire larger (`T-FIRE-RADIUS-OIL`, 5 m) and every oil drop it reaches ignites and is destroyed. The player survives only by getting more than 5 m from the burst, for example by jumping to the opposite wall's pipe shadow, before the fire spreads.
- A cracked crack point can't be cracked again. `[Default]`
- Oil hides you from guards but not from light: a guard's lantern still drains you in an oil-drop shadow.

### 7.7 Flame light

| Source | Shape | Effect | Where | When |
|---|---|---|---|---|
| Guard lantern | Sphere around the guard, radius `T-LANTERN-RADIUS` (3 m) | −8/s while it overlaps your shadow, even opaque shadows. Never hurts the shadow its own guard casts. | Everywhere guards are | Both states |
| Alarm light | Cone from ceiling, floor radius 2 m | −5/s | 2 in Main Corridor, 2 in Access Corridor, placed over open floor near (not on) the guard routes | Meltdown only |
| Wall fire lantern | Box volume filling the Final Corridor | Instant death, even in shadow | Final Corridor walls | Meltdown only |
| Lantern fire | Disc growing from the smash point (section 7.9) | Instant death on contact, escapable | Wherever a thrown lantern lands | On spot / burst |
| Ceiling lights | – | Harmless | Everywhere | Both |

Each damaging light = a Unity Light for looks + a trigger volume with a `FlameVolume` component (`damagePerSecond` or `instantKill`). Turning a light off disables both.

### 7.8 Semi-Final Gate (guard-only)

- A two-panel sliding gate. It opens when any guard enters its 2 m trigger zone on either side, and closes 1.5 s after the last guard leaves.
- It never opens for the player alone. The gate panels are solid walls to jump raycasts.
- Its doorway also blocks the entity's jump raycast while closed, so the only way through is inside a guard's shadow.
- Same behaviour in both states.

### 7.9 Lantern throw and fire

Confirmed by Haider: a guard that spots you directly smashes its lantern, but you live if you avoid the flames.

1. **Alert** (`T-GRD-ALERT`, 0.6 s): the guard stops, turns to you, "!" icon and alert sting.
2. **Throw:** the lantern flies in an arc to where your shadow was when the throw started (`T-THROW-TIME`, 0.6 s). It does not home in on you.
3. **Impact:** the lantern smashes into a fire disc of radius `T-FIRE-START` (1 m) that grows to `T-FIRE-RADIUS` (4 m) over `T-FIRE-GROW` (2 s), burns for `T-FIRE-LIFE` (6 s), then shrinks out over 1 s.
4. **Contact = death:** if the fire's edge touches the entity's shadow (including riding, but not while airborne mid-jump), Essence drops to 0.
5. **Escape:** you have about 1.2 s before impact, plus the growth time, to get clear: jump to a shadow more than 4 m from the impact point, or ride a guard out of reach. A burning fire also blocks jumps whose landing spot is inside it (target shows red).
6. Any oil drop or void shadow the fire reaches is destroyed.
7. The guard is **lanternless** for `T-LANTERN-RELIGHT` (10 s), then lights a spare.

Other guards don't react to the fire `[Default]`; only a guard that spots you directly throws.

---

## 8. Guards

### 8.1 Representation

A capsule (greybox) with a head and a lantern held to the front-right, a floor view-cone decal, and an attached opaque `ShadowArea` behind them. Walk speed `T-GRD-WALK` (2 m/s). Moved with `NavMeshAgent` or simple waypoint lerp (`[Default]`: waypoint lerp, no NavMesh, cheaper and deterministic for reverse).

### 8.2 AI state machine

| State | Behaviour | Exit |
|---|---|---|
| **Travel** | Walks a waypoint route from A to B. Appends its position to **history** every 0.25 s. | Reaches end → Post; reversed → Reversing |
| **Post** | Loops a short patrol at its destination (pacing, looking around, scripted look-at points). Does **not** append history. | Reversed → Reversing |
| **Reversing** | Walks to the last history point, then backwards through history at 2× speed. Removes points as it passes them. | R released / history empty / floor hit → Hold |
| **Hold** | Stands still, cone active. | Reversed again → Reversing |
| **Alert** | Spotted the player: stops, turns to the player, "!" icon and alert sting, `T-GRD-ALERT` (0.6 s). | → Throw |
| **Throw** | Throws the lantern at the player's position at that moment (section 7.9). Lantern light goes with it. | → Lanternless |
| **Lanternless** | Resumes its previous state (Travel/Post/Hold) with no lantern: no lantern light, can't throw. View cone still active but spotting does nothing. After `T-LANTERN-RELIGHT` (10 s) it lights a spare and returns to normal. | Timer → previous state with lantern |
| **BeingConsumed** | Frozen, sinking. | Destroyed |

History is a list of positions recorded live from the moment the guard spawns. Because a death always restarts the whole game, histories never need to be restored or seeded.

### 8.3 Perception

- View cone: `T-VIEW-ANGLE` (70° total) and `T-VIEW-RANGE` (8 m), from head height, line of sight checked with a raycast against walls and the gate.
- The cone is drawn on the floor as a faint translucent decal so the player can read it (MUST, the puzzles depend on it).
- **Spotted** if the entity is inside the cone with line of sight AND any of:
  - the entity is **jumping**, or
  - the entity is **consuming** or channelling the facility rewind, or
  - the entity is in a **soft** shadow (pipes).
- Spotting requires the condition to hold for `T-VIEW-GRACE` (0.2 s) continuously, to avoid one-frame unfair deaths.
- In an opaque shadow and not acting, you are never spotted.
- Other triggers for Alert: a pipe burst within the Final Corridor (every guard there with a lantern throws at the pipe).
- Only guards carrying a lit lantern can spot you; a lanternless guard ignores you `[Default]`.

### 8.4 Spawners

| Spawner | State | Spawns at | Route | Interval | Max alive |
|---|---|---|---|---|---|
| SP-SLEEP-M | Meltdown | D-SLEEP | Barracks → D-BARRACKS → Main Corridor to Z = 14 → back → despawn at D-SLEEP | 20 s | 2 |
| SP-SLEEP-N | Normal | D-SLEEP | Barracks/Lab routine loop (section 10) | 25 s | 3 total in Barracks+Lab |
| SP-SIDE-N | Normal | D-SIDE-W and D-SIDE-E alternately | Side corridor → Final Corridor → G-SEMI → Main Corridor → D-BARRACKS → despawn at D-SLEEP | 15 s | 2 in transit |
| SP-FINAL-N | Normal | D-SIDE-W / D-SIDE-E | Final Corridor patrol loop (Z 43 to 54) | Refill to 2 patrollers, 10 s after one is consumed | 2 |
| SP-FINAL-M | Meltdown | D-SIDE-E | Final Corridor patrol (scenery, unreachable) | – | 2 |

Spawners mean the player can never run out of guards to eat. Guards **flow one way** through the Semi-Final Gate in the Normal state (Final Corridor → Main Corridor), so getting into the Final Corridor requires reversing one (puzzle 4).

---

## 9. Puzzle sequence and level script

Each beat lists the setup, the intended solution, fail states, and acceptance criteria an implementer can test.

### Beat 0: Intro (Meltdown)
**Script (non-interactive, about 20 s, skippable with Space after first play):**
1. Black screen, then alarm lights flash red; klaxon. All containment doors slide open (D-SCP-T, D-SCP-A..D). World state = Meltdown.
2. Guard wave (section 9.1 table) leaves the Barracks through D-BARRACKS one by one, every 3 s, heading down the Main Corridor.
3. The first guard (G-GLANCE) stops at D-PLAYER, turns and holds the lantern into the room for 2 s. The light grazes the entity: −10 Essence, screen flash. G-GLANCE moves on.
4. The entity "wakes": the camera pulls back from a close shot of the black shadow to the gameplay camera. Control is given.

**Acceptance:** after the intro, Essence = 90, the player is in the Player Room shadow with control, and the remaining wave guards are about to pass D-PLAYER.

### 9.1 Meltdown guard roster

| Guard | Spawn | Route (Travel) | Post |
|---|---|---|---|
| G-GLANCE | Barracks | Main Corridor, pause at D-PLAYER, → Access Corridor → outside SCP A door | Pace 3 m outside SCP A |
| G-B | Barracks | → outside SCP B door | Pace outside SCP B |
| **G-TIME** | Barracks (by the lantern rack) | D-BARRACKS → Main Corridor (along the **west** half, passing within 3 m of D-PLAYER) → Access Corridor → D-SCP-T → into the Time SCP Room | Loop around the pedestal (section 9.3) |
| G-C | Barracks | → outside SCP C door | Pace outside SCP C |
| G-D | Barracks | → outside SCP D door | Pace outside SCP D |
| G-MAIN | Barracks (leaves last) | – | Paces the Main Corridor, Z 14 to 38 |
| G-BARRACKS | Barracks | – | Paces between the lantern rack and the lever, faces the lever every 8 s |
| SP-SLEEP-M guards | See 8.4 | | |

Wave order past D-PLAYER: G-GLANCE, G-B, **G-TIME**, G-C, G-D, with 3 s spacing. Only G-TIME walks the west half of the corridor, close to the player's door; the others walk the east half, out of jump range (> 6 m) of the doorway `[Default]`. G-TIME also carries a distinct **amber** lantern and the Time Room door has an amber stripe, so the player can read the destination.

### Beat 1: Catch a ride
- **Goal:** get to the Time SCP Room.
- **Solution:** from the Player Room's doorway, jump into G-TIME's shadow as it passes. Ride it down the Main Corridor and Access Corridor into the Time SCP Room.
- **Fail (soft):** if all five wave guards have reached their posts and the player is not in the Time SCP Room, show "You missed your chance" and a restart prompt (auto-restart after 3 s). Riding a wrong guard just leads to this.
- **Hazards:** alarm lights along the Access Corridor sit off G-TIME's route `[Default]`, so a clean ride costs nothing.
- **Teaches:** jumping, moving shadows, timing, reading destinations.
- **Acceptance:** riding G-TIME from the doorway delivers the entity into the Time SCP Room with no player input beyond one jump.

### Beat 2: Steal the stopwatch
- **Setup:** G-TIME loops a 5 × 4 m rectangle around the pedestal, facing along its path, with one 3 s stop at the north-east corner looking at the pedestal. Loop period about 12 s. The cabinet shadow is near the door.
- **Solution:** hop off G-TIME into the cabinet shadow (when its back is turned), wait until G-TIME turns away, jump to the pedestal shadow, hold E for 3 s while the pedestal is outside its cone.
- **Design check:** the loop must give at least one window of ≥ 4 s per cycle where both the cabinet-to-pedestal jump path and the pedestal are outside G-TIME's cone.
- **Fail:** spotted → lantern thrown → caught by the fire → death → restart. If you escape the fire, the puzzle carries on and G-TIME is lanternless for 10 s.
- **Reward:** `hasTimePower = true`, HUD reveals R/Q hints, short stinger and a one-line tutorial: "Hold R while riding a guard to reverse their time."
- **Acceptance:** consuming the pedestal unseen sets Essence −20 and unlocks time powers; consuming it inside the cone kills the player.

### Beat 3: The lever
- **Goal:** turn off the Final Corridor's wall fire lanterns.
- **Solution:**
  1. Jump into G-TIME's shadow unseen (from behind).
  2. Hold R. G-TIME walks back: out of the Time Room, along the Access Corridor, up the Main Corridor, into the Barracks (its full history, about 55 m, about 14 s at 2×, −70 Essence).
  3. The player will hit the floor (10) around the Main Corridor. G-TIME goes on Hold. Hop off, ride or ambush G-MAIN or an SP-SLEEP-M guard from behind, consume it (+25), jump back into G-TIME's shadow, continue reversing.
  4. G-TIME ends at its spawn point by the lantern rack, within jump range of the lever panel. Wait for G-BARRACKS to face away, jump into the lever's shadow, hold Q for 2 s.
- **Result:** facility rewind → Normal state (section 10). The lever is shown moving down; the wall fire lanterns go out.
- **Fail:** caught by lantern fire after being spotted, or Essence 0 from lantern light → restart.
- **Acceptance:** reversing G-TIME returns it along the exact path it walked in, through the doors; releasing R leaves it on Hold; the rewind only works from the lever's shadow, in Meltdown, with time power.


### Beat 4: The Semi-Final Gate
- **Setup (Normal):** relaxed guards in the Barracks and Lab. SP-SIDE-N guards walk from the side corridors, through G-SEMI, down the Main Corridor and through the Barracks to D-SLEEP.
- **Solution:** eat a Barracks guard if Essence is low. Jump into the shadow of an SP-SIDE-N guard after it has come through G-SEMI (in the Main Corridor or Barracks). Hold R: it walks backwards through G-SEMI (the gate opens for it) into the Final Corridor. Hop off into a pipe shadow before its history ends at the side corridor.
- **Cost:** about 20 m reversed ≈ 5 s ≈ −25 Essence.
- **Fail:** caught by lantern fire after being spotted → restart.
- **Acceptance:** the gate never opens for the player alone; a reversed guard opens it; after reversing through, the player can reach the Final Corridor floor.

### Beat 5: Oil and the Final Gate
- **Setup:** Final Corridor with harmless ceiling lights, pipes on both walls (soft shadows), 2 patrolling guards (SP-FINAL-N) walking Z 43 to 54 and back, offset by half a loop. No timer.
- **Solution:** move along pipe shadows only when no guard is looking that way. Crack pipes to create oil drops (opaque hiding spots). Hide in oil while a guard passes, then jump into its shadow from behind and consume it. Repeat until Essence > 80. Reach the Final Gate's shadow and hold E for 5 s unseen.
- **Fail:** caught by lantern fire (after being spotted in a pipe shadow, or after a pipe burst), or lantern drain → restart.
- **Win:** the gate sinks into the void, the entity flows out, cut to the win screen ("NULLFLAME — Contained: no").
- **Acceptance:** the gate consume is refused at Essence ≤ 80; pipe crack in the safe zone spawns 3 opaque drops; holding to 1.5 s triggers the burst and its oil fire.

---

## 10. World states

Implemented as two parent objects, `State_Meltdown` and `State_Normal`, each holding that state's lights, damage volumes, door poses, lever pose, guards and spawners. Shared geometry lives outside both. The facility rewind disables one and enables the other behind the rewind screen effect (1.5 s white-out/scanline sweep). Guards from the old state are destroyed; the player stays where they are.

| | **Meltdown** (Beat 0 to 3) | **Normal** (Beat 4 to 5) |
|---|---|---|
| SCP room doors | All open | All closed |
| Alarm lights + klaxon | On | Off |
| Final Corridor wall fire lanterns | On (instant kill) | Off |
| Ceiling lights | On | On |
| Breaker lever | Up | Down |
| Semi-Final Gate | Guard-only | Guard-only |
| Guards | Wave roster (9.1), G-MAIN, G-BARRACKS, SP-SLEEP-M, SP-FINAL-M | Barracks/Lab routine (2 guards + SP-SLEEP-N), Main Corridor patroller (Z 14 to 38), SP-SIDE-N, SP-FINAL-N |
| Guard mood | Alert, fast animation, walk 2.4 m/s `[Default]` | Relaxed, walk 2 m/s, pauses at benches/bunks |
| Music | Alarm-driven tension loop | Low hum |

Rules for both states: guards carry flame lanterns, never enter the four other SCP rooms, and arrive and leave through the show-only doors.

---

## 11. Camera

- Third-person orbit camera following the entity (Cinemachine `OrbitalFollow` or a simple custom rig). Distance 6 m, pitch 25° to 70° (default 45°), so the player can look down at shadows and guard cones.
- Follows the entity during jumps with slight damping.
- Collides with walls (pulls in).
- Centre reticle used for jump targeting.
- During Beat 0 the camera is scripted.

---

## 12. UI and feedback

| Element | Spec |
|---|---|
| Essence bar | Top-left, black-to-violet bar with a numeric value. Shows a ghost segment of the cost while a hold action is previewed. Pulses below 25. |
| Reticle | Centre dot. |
| Target highlight | White outline on the targeted shadow; red if out of range or blocked. |
| Context prompt | Bottom-centre: e.g. `[E] Consume Guard (+25)`, `[R] Reverse (5/s)`, `[Q] Rewind facility (30)`. |
| Hold ring | Radial ring around the reticle while holding E/R/Q; pipe meter shows grey/green/red zones. |
| Guard awareness | "!" over the guard on Alert. |
| Damage feedback | Orange vignette while in flame light; red flash on hit. |
| Time effect | Desaturation + scanlines while any time power runs. |
| Death screen | "Burned away." · `Restart` (auto after 3 s). Restarting reloads the scene and skips straight to Beat 1. |
| Win screen | Title card + time taken. Return to title. |
| Title screen | `Nullflame` · Play · Quit. |
| Tutorial hints | One-line hints at first opportunity: move (Beat 0), jump (Beat 1), consume (Beat 2), reverse (Beat 3), rewind (at the lever), crack pipe (Beat 5). |

---

## 13. Tuning table

| ID | Value | Meaning |
|---|---|---|
| T-LVL-CEIL | 4 m | Ceiling height |
| T-PLY-SIZE | 1.2 m | Entity decal diameter |
| T-JMP-RANGE | 6 m | Max jump distance |
| T-JMP-ARC | 1.5 m | Jump apex height |
| T-JMP-TIME | 0.45 s | Jump duration |
| T-ESS-MAX | 100 | Essence max |
| T-ESS-START | 100 | Essence at new game |
| T-ESS-INTRO | 10 | Intro graze damage |
| T-ESS-FLOOR | 10 | Held time powers stop here |
| T-DMG-LANTERN | 8/s | Guard lantern drain |
| T-DMG-ALARM | 5/s | Alarm light drain |
| T-LANTERN-RADIUS | 3 m | Guard lantern damage radius |
| T-THROW-TIME | 0.6 s | Lantern flight time |
| T-FIRE-START | 1 m | Fire radius at impact |
| T-FIRE-RADIUS | 4 m | Lantern fire max radius |
| T-FIRE-RADIUS-OIL | 5 m | Fire max radius at a pipe burst |
| T-FIRE-GROW | 2 s | Fire growth time to max radius |
| T-FIRE-LIFE | 6 s | Fire burn time before it dies out |
| T-LANTERN-RELIGHT | 10 s | Time a guard is lanternless after throwing |
| T-COST-PIPE | 5 | Pipe crack cost |
| T-COST-PEDESTAL | 20 | Pedestal + Time SCP cost |
| T-COST-GATE | 80 | Final Gate cost |
| T-COST-REVERSE | 5/s | Local reverse cost |
| T-COST-REWIND | 30 | Facility rewind cost |
| T-GAIN-GUARD | 25 | Guard consume gain |
| T-CON-GUARD | 2 s | Guard consume time |
| T-CON-PEDESTAL | 3 s | Pedestal consume time |
| T-CON-GATE | 5 s | Gate consume time |
| T-REWIND-CHANNEL | 2 s | Facility rewind hold time |
| T-REV-SPEED | 2× walk | Reverse playback speed |
| T-PIPE-BURST | 1.5 s | Pipe hold time to burst (safe zone 0.6 to 1.1 s) |
| T-GRD-WALK | 2 m/s | Guard walk speed (Normal); Meltdown 2.4 m/s |
| T-GRD-ALERT | 0.6 s | Alert before the throw |
| T-VIEW-ANGLE | 70° | View cone total angle |
| T-VIEW-RANGE | 8 m | View cone range |
| T-VIEW-GRACE | 0.2 s | Time in view before spotted |

---

## 14. System breakdown (for the implementation docs)

Each row is a candidate implementation unit. Dependencies point upward in the build order (section 15).

| # | System | Responsibility | Key components / data | Depends on |
|---|---|---|---|---|
| S1 | **Level greybox** | Rooms, walls, doors, gate, props from section 4 | Scene `Facility`, ProBuilder or primitive cubes | – |
| S2 | **GameTuning** | All tunables | ScriptableObject | – |
| S3 | **Shadow system** | `ShadowArea` component, registry of all active shadows, queries (in range, under ray) | `ShadowArea`, `ShadowRegistry` | S1 |
| S4 | **Player entity** | State machine, slide in shadow, jump, riding, visuals | `ShadowEntity`, `JumpTargeting` | S3 |
| S5 | **Camera** | Orbit follow, reticle | Cinemachine or custom | S4 |
| S6 | **Essence** | Value, change events, floor/afford rules, death trigger | `Essence` | S2 |
| S7 | **Guard movement** | Waypoint routes, Travel/Post/Hold states, history | `GuardController`, `WaypointRoute` | S1 |
| S8 | **Guard perception** | View cone check, floor cone decal, spotted events | `GuardVision` | S4, S7 |
| S9 | **Flame volumes** | Lantern / alarm / wall-lantern volumes, damage per second, kill | `FlameVolume` | S6 |
| S10 | **Alert and lantern throw** | Alert → throw → growing fire that kills on contact → guard lanternless, relights | `LanternAttack`, fire prefab | S8, S9 |
| S11 | **Consume** | Consumable component, hold-to-consume, sink, void shadow, events | `Consumable`, `ConsumeAction` | S3, S4, S6, S8 |
| S12 | **Time powers** | Reverse, facility rewind, screen effect | `TimePowers`, `GuardController.Reverse()` | S6, S7, S11 |
| S13 | **World states** | Meltdown/Normal roots and swap | `WorldStateController` | S1, S7, S9 |
| S14 | **Guard-only gate** | Open for guards, block player | `GuardGate` | S7 |
| S15 | **Spawners** | Timed guard spawns with caps | `GuardSpawner` | S7 |
| S16 | **Oil pipes** | Crack meter, drops, burst alert | `CrackablePipe`, `OilDrop` | S3, S6, S10 |
| S17 | **Game flow** | Title, intro, death → full scene reload (intro skipped on retries), win | `GameFlow` | most |
| S18 | **Intro sequence** | Scripted Beat 0 | Timeline or coroutine | S7, S13 |
| S19 | **UI** | HUD, prompts, rings, screens | uGUI or UI Toolkit | S6 |
| S20 | **Audio / VFX** | Klaxon, footsteps, alert sting, consume, rewind, fire | – | – |

**Restart implementation:** no checkpoints. Death reloads the `Facility` scene. A static flag (`GameFlow.HasSeenIntro`) survives the reload so retries skip the intro and start at Beat 1 with Essence 90.

---

## 15. Build order and cut list

Build order (each step playable before the next):

1. S1, S2: greybox the facility from section 4. Third-person camera (S5).
2. S3, S4: static shadows, entity decal, slide, jump with teardrop.
3. S7, S8: guards on waypoint routes with view cones. Riding.
4. S6, S9, S10, S19 (bar only): flame damage, lantern attack, death and reload to a fixed spawn.
5. S11: consume guards and objects; pedestal grants time power.
6. S12 reverse + S14 gate.
7. S13 world states + facility rewind at the lever.
8. S15 spawners, S17 game flow, Beats 1 to 4 scripted end to end.
9. S16 oil pipes, Final Gate, win screen.
10. S18 intro, S20 audio/VFX, tutorial hints, polish.

Cut first, in this order, if time runs short:
1. Lab interior and the other SCP rooms' interiors (doors open onto a dark backdrop).
2. Pipe burst fail case (keep cracking, cap the meter at the safe zone).
3. Fancy consume/rewind VFX (scaling black decal + screen filter only).
4. Intro scripting (replace with a one-line "Containment breach" card and start at Beat 1, with Essence 90).
5. Alarm light damage (keep them as visuals).

Never cut: shadow jump, riding, view cones, consume, local reverse, facility rewind, guard-only gate, oil drops, Final Gate.

---

## 16. Contradictions found, defaults chosen, and open questions

### 16.1 Contradictions in the sources (resolved by the latest word)

| # | Earlier text | Latest word (used here) |
|---|---|---|
| C1 | First brief: the lever turns off the electricity/lights in the **main corridor**. | The lever turns off the **Final Corridor's wall fire lanterns** (Haider, 05:07). |
| C2 | Second brief: after the rewind the Semi-Final Gate is "basically open". | The gate **only opens for guards**, in both states (Haider, 05:00). |
| C3 | Thread 05:00: puzzle 5 is time-based; strong lights hurt you through soft pipe shadows. | No timer and no light damage through pipes; oil is only for hiding from guards (Haider, 05:07). |
| C4 | Second brief: "once we have done the final battle". | There is no final battle (Haider, 05:00). |
| C5 | Ideation 05: reverse records guard positions every frame. | Kept, sampled every 0.25 s and only while travelling, so a guard on its patrol loop reverses straight back onto its route. |

### 16.2 Contradictions still open (defaults in place)

| # | Issue | Default used |
|---|---|---|
| O1 | Guard lanterns hurt you "even in shadow", but riding a guard's shadow is the main way to travel, which would put you under its lantern all the time. | A guard's own lantern never hurts the shadow it casts. |
| O2 | Ideation 03 and Haider's first brief call alarm lights dangerous, but no puzzle places them. | Alarm lights deal small damage at 4 fixed spots off the main routes, Meltdown only. Easy to set to 0. |
| O3 | Ideation 04 says long rewinds and big consumes "can kill you"; that makes it easy to soft-lock or die on a button press. | Spending can never kill you: consumes need Essence > cost, held powers stop at 10. Only damage kills. |
| O4 | "Every guard passes and you missed" is the puzzle 1 fail, but nothing says how the player tells the Time Room guard apart. | G-TIME walks closest to the player's door and carries an amber lantern matching an amber stripe on the Time Room door. |
| O5 | 08 lists "one guard inside the Time SCP Room" in Meltdown, while 01 says guards never enter SCP rooms. | Only the four *other* SCP rooms are off-limits; G-TIME enters the Time Room. |

### 16.3 Open questions for Haider

1. ~~**Final Gate cost:**~~ Answered: 80 of a 100 bar, no bar growth (Haider, 2026-10-17).
2. ~~**Controls:**~~ Answered: camera-aim plus left-click picks jump targets (Haider, 2026-10-17).
3. ~~**Checkpoints:**~~ Answered: no checkpoints, death restarts the game (Haider, 2026-10-17).
4. ~~**Stop time:**~~ Answered: cut (Haider, 2026-10-17).
5. ~~**Speed up:**~~ Answered: cut (Haider, 2026-10-17).
6. ~~**Who triggered the meltdown?**~~ Answered: no backstory; the meltdown is never explained (Haider, 2026-10-17).
7. ~~**Spotted = death:**~~ Answered: the spotting guard throws its lantern; you live if you escape its flames (Haider, 2026-10-17).
8. ~~**Scale:**~~ Answered: 1 px = 0.05 m is right (Haider, 2026-10-17).
