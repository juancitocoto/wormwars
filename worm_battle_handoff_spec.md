# Handoff Spec: Battle Screen — Worm Battle (v1)

**Status:** Design locked. Numeric balancing values marked `[TUNE]` are placeholders pending playtest.
**Platform:** Mobile app, **landscape only**, two-handed.
**Reference device for all dp values:** 844 × 390 dp (standard phone, landscape). All fixed values below are given in dp at this reference; see [Responsive Behavior](#responsive-behavior) for scaling rules.

---

## Overview

The battle screen is the core gameplay view and the only screen the player spends sustained time in. Two mirrored castles face each other across an open courtyard, each rendered as a **dollhouse-style cutaway** — the front wall is absent so the worms inside and the destruction happening to them stay visible at all times. Players take alternating timed turns, aiming with a drag gesture and firing at enemy worms and/or the enemy castle.

**A team wins only when the enemy team is eliminated *and* the enemy castle is destroyed.** Both conditions are required, which means the UI must communicate two independent progress tracks (worms remaining, castle HP) at all times without either crowding the battlefield.

**Design intent to preserve:** the battlefield is the hero. Every piece of chrome is either pinned to the top edge or the bottom 30% control band — nothing floats over the mid-screen play area except transient combat effects. If a layout decision is ambiguous, favor the option that keeps the middle of the screen clear.

---

## Layout

### Vertical zones (percentages of screen height, top to bottom)

| Zone | Height | Contents | Notes |
|---|---|---|---|
| Top HUD | 0–11% | Turn badge, timer, worm count | Pinned to top safe area |
| Castle HP row | 11–19% | Two castle HP bars | Left-aligned and right-aligned |
| Battlefield | 19–70% | Sky, castles, worms, projectiles | **Never overlaid by persistent UI** |
| Ground | 70–78% | Dirt strip + castle footings | Castles sit on this line |
| Control band | 70–100% | Joystick, weapon tray, fire button | Overlaps ground visually; controls are on top layer |

The control band and ground zone deliberately overlap — the controls sit *over* the lower dirt area rather than pushing the battlefield up. This buys ~8% more battlefield height on small screens.

### Horizontal layout

- **Castles:** each occupies `33%` of screen width, inset `1.5%` from its respective screen edge. Left castle = Team A, right castle = Team B. The two are mirror images geometrically, not identical assets — the cutaway opening faces inward (toward the courtyard) on both sides.
- **Courtyard:** the remaining `~31%` center gap. This is the minimum projectile flight corridor; it must never compress below `spacing-courtyard-min` (240dp) or trajectory arcs become unreadable.
- **Controls:** joystick pinned left (`spacing-lg` from edge), fire button pinned right (`spacing-lg` from edge), weapon tray centered horizontally.

### Safe areas

Landscape notches/cutouts sit on the **left or right edge** depending on device rotation. All edge-pinned elements (turn badge, worm count badge, joystick, fire button) must respect `safe-area-inset-left` and `safe-area-inset-right` in addition to their spacing tokens. The castle at `1.5%` inset may be clipped by a notch — this is acceptable for decorative stone, but **castle HP bars and worm sprites must never fall inside a cutout region.**

---

## Design Tokens Used

### Color — Structure & world

| Token | Value | Usage |
|---|---|---|
| `color-outline` | `#3d2b1f` | Universal outline on every game object. 3–5dp stroke. Non-negotiable — it's the visual signature of the style. |
| `color-sky-top` | `#7fd4ff` | Top of sky gradient |
| `color-sky-bottom` | `#cdf1ff` | Bottom of sky gradient |
| `color-stone` | `#b7b0a6` | Castle outer wall fill |
| `color-stone-dark` | `#8d867c` | Castle wall inner bevel, rubble |
| `color-interior` | `#f3e6c8` | Castle interior back wall (the "cut open" surface) |
| `color-interior-dark` | `#e3d1a4` | Interior brick texture lines |
| `color-dirt` | `#a5672f` | Ground surface |
| `color-dirt-dark` | `#7a4720` | Ground subsurface, ground top edge |
| `color-wood` | `#c98a4b` | Joystick base, UI bevels |
| `color-wood-dark` | `#8a5a2b` | Drop shadow under cream UI elements, castle floor beam |

### Color — Teams

| Token | Value | Usage |
|---|---|---|
| `color-team-a` | `#6a5cff` | Team A worm body, turn badge, joystick knob, flag |
| `color-team-a-dark` | `#4636c9` | Team A drop shadows |
| `color-team-b` | `#ff8a3d` | Team B worm body, flag |
| `color-team-b-dark` | `#d9631a` | Team B drop shadows |

> **Note:** Team colors are **placeholder**. They currently pass contrast against sky and stone, but final palette selection is pending. Implement them as swappable theme values, not hardcoded — team color customization is already on the roadmap via the team/worm customization screen.

### Color — UI & feedback

| Token | Value | Usage |
|---|---|---|
| `color-cream` | `#fff3d6` | Badge and button surface |
| `color-fire` | `#ff5a5a` | Fire button fill |
| `color-fire-dark` | `#c73a3a` | Fire button drop shadow |
| `color-hp-fill-start` | `#8ce05a` | Worm HP bar gradient, top |
| `color-hp-fill-end` | `#4fae2e` | Worm HP bar gradient, bottom |
| `color-hp-track` | `#4a2e1e` | Worm HP bar empty track |
| `color-castle-hp-start` | `#ffd873` | Castle HP bar gradient, top |
| `color-castle-hp-end` | `#e0a52c` | Castle HP bar gradient, bottom |
| `color-shield` | `#8fd8ff` | Initial-impact-protection shield cue (see open items) |
| `color-selected-slot` | `#ffe27a` | Selected weapon slot fill |
| `color-selected-glow` | `#fff59a` | Selected weapon slot inner glow |
| `color-locked` | `#b3aeb0` | Locked weapon slot fill |
| `color-locked-fg` | `#6d6a6c` | Locked weapon slot icon |

Castle HP uses **gold**, worm HP uses **green** — deliberately different hues so a glance distinguishes the two win-condition tracks. Do not unify these.

### Spacing

| Token | Value | Usage |
|---|---|---|
| `spacing-xs` | 4dp | Icon-to-label gaps |
| `spacing-sm` | 8dp | Between weapon slots, inside badges |
| `spacing-md` | 12dp | Badge padding, tray padding |
| `spacing-lg` | 16dp | Screen edge insets for controls |
| `spacing-xl` | 24dp | Between major HUD groups |
| `spacing-courtyard-min` | 240dp | Minimum center gap between castles |

### Typography

| Token | Size / Weight | Usage |
|---|---|---|
| `font-badge-label` | 11dp / 800 | "TEAM A", worm count |
| `font-badge-sub` | 9dp / 600 | "Worm 2 of 3" secondary line |
| `font-timer` | 15dp / 800 | Turn countdown |
| `font-control-caption` | 9dp / 700 | "AIM / POWER", "RELEASE" |
| `font-castle-hp-label` | 9dp / 800 | "A CASTLE", "B CASTLE" |

Typeface is a rounded, heavy display face (Baloo 2 or equivalent) to match the chunky style. Never use a weight below 600 anywhere on this screen — thin type reads as a different game.

### Elevation / Shadow

| Token | Value | Usage |
|---|---|---|
| `shadow-button` | `0 6dp 0 {dark-variant}` | Joystick, fire button — hard offset, no blur |
| `shadow-badge` | `0 4dp 0 {dark-variant}` | Badges, weapon slots |
| `shadow-worm` | ellipse, 28×6dp, `rgba(0,0,0,0.2)` | Contact shadow under each worm |

**Shadows in this style are hard-edged solid offsets, never blurred.** A blurred shadow anywhere on this screen is a bug.

### Border radius

| Token | Value | Usage |
|---|---|---|
| `radius-pill` | 999dp | HP bars |
| `radius-slot` | 10dp | Weapon slots |
| `radius-badge` | 14dp | HUD badges |
| `radius-tray` | 16dp | Weapon tray container |
| `radius-circle` | 50% | Joystick, fire button, worm bodies |

---

## Components

| Component | Variant | Props | Notes |
|---|---|---|---|
| `TurnBadge` | `active` / `waiting` | `teamId`, `wormIndex`, `wormTotal` | Fills with the active team's color; `waiting` variant uses `color-cream` with outline. Only one is ever `active`. |
| `TurnTimer` | `normal` / `urgent` | `secondsRemaining` | Switches to `urgent` at ≤10s — see States table. |
| `WormCountBadge` | — | `teamAAlive`, `teamBAlive` | Format: `3–2`. Counts **alive** worms, not total. |
| `CastleHPBar` | `default` / `critical` | `teamId`, `currentHP`, `maxHP` | `critical` at ≤25%. No shielded variant — see Resolved note in Open Items. |
| `WormSprite` | `idle` / `active` / `aiming` / `hit` / `smoking` / `eliminated` | `teamId`, `hp`, `maxHp`, `isActive` | `active` gets a star marker + subtle idle bob. See Animation. |
| `WormHPPill` | `default` / `critical` | `hp`, `maxHp` | Floats 14dp above worm sprite. Always visible (decision: no hover/on-hit-only reveal). |
| `Castle` | `intact` / `shockwave` / `smoking` / `rubble` / `breached` / `destroyed` | `teamId`, `hpPercent`, `upgradeTier`, `hitCount` | Damage stage is derived from `hpPercent`, not set directly. `hitCount` drives the weapon escalation curve. See Castle Damage States. |
| `AimJoystick` | `idle` / `dragging` / `charging` | `angle`, `power`, `maxPower` | Fixed position, not floating. Knob tinted with active team color. |
| `WeaponTray` | — | `weapons[]`, `selectedIndex` | 4 slots in v1 (3 unlocked + 1 locked placeholder). |
| `WeaponSlot` | `default` / `selected` / `locked` / `depleted` | `icon`, `ammo`, `isSelected`, `isLocked` | `selected` lifts 2dp and gains inner glow. |
| `FireButton` | `ready` / `charging` / `disabled` | `onPressStart`, `onRelease` | Disabled during projectile flight and opponent's turn. |
| `WindIndicator` | — | `direction`, `strength` | Top-center. Arrow rotates; strength shown by arrow scale + count. |
| `TrajectoryArc` | — | `points[]`, `visible` | Dashed arc, only rendered while `AimJoystick` is `dragging` or `charging`. |

---

## States and Interactions

### Turn flow states

| State | Duration | Controls | Camera |
|---|---|---|---|
| `turn-start` | 600ms | Locked | Pans to active worm, settles |
| `player-aiming` | until fire or timeout | Joystick + tray + fire active | Follows active worm, player can pan by dragging battlefield |
| `projectile-flight` | variable | All locked | Follows projectile |
| `impact-resolve` | 800–2000ms | All locked | Holds on impact point |
| `turn-end` | 400ms | Locked | Begins pan to next worm |
| `opponent-turn` | full opponent turn | All locked, dimmed 40% | Follows opponent action |

### Element states

| Element | State | Behavior |
|---|---|---|
| `FireButton` | `ready` | Full `color-fire`, `shadow-button` at 6dp |
| `FireButton` | `pressed/charging` | Depresses to 2dp shadow offset, power meter fills around circumference clockwise |
| `FireButton` | `released` | Snaps back to 6dp over 120ms, fires at charged power |
| `FireButton` | `disabled` | 45% opacity, no shadow, ignores input |
| `AimJoystick` | `idle` | Knob centered |
| `AimJoystick` | `dragging` | Knob follows thumb within 32dp radius; angle = knob vector; `TrajectoryArc` appears |
| `AimJoystick` | `released without fire` | Knob springs back over 200ms; arc persists 400ms then fades |
| `WeaponSlot` | `default` | Cream fill, 4dp shadow |
| `WeaponSlot` | `pressed` | Depresses to 1dp shadow for 80ms |
| `WeaponSlot` | `selected` | `color-selected-slot` fill, translateY -2dp, 3dp inner glow, shadow stays 4dp |
| `WeaponSlot` | `locked` | `color-locked` fill, lock icon, tap shows "Unlock in Battle Pass" toast — **does not** deep-link out of an active match |
| `TurnTimer` | `normal` | Cream badge, `font-timer` |
| `TurnTimer` | `urgent` (≤10s) | Badge pulses scale 1.0→1.08→1.0 every 1s, digits shift to `color-fire` |
| `TurnTimer` | `expired` | Turn auto-passes, no shot fired (decision: skip turn, no random action) |
| `WormSprite` | `active` | Star marker above, idle bob ±2dp / 2s loop |
| `WormSprite` | `hit` | Flash white 100ms, knockback, then enters `smoking` |
| `WormSprite` | `eliminated` | Smoke builds 400ms → comic explosion burst → sprite removed, HP pill removed |
| `CastleHPBar` | `critical` (≤25%) | Bar pulses, castle silhouette gains persistent smoke |
| `Castle` | first hit | Shockwave ripple expands from impact point across the wall face, dust puff, light scorch decal. Small HP drain. No geometry change. |

### Castle damage states

Damage stage is **derived from HP percentage**, not incremented per hit — this keeps it consistent whether a castle was chipped by starter weapons or cut by a laser saw.

| HP remaining | Stage | Visual |
|---|---|---|
| 100% (untouched) | `intact` | Clean walls |
| 99–85% | `shockwave` | Ripple/dust-puff across the struck wall face, light scorch mark at impact point. **No geometry change.** This is the "minimal damage" read — the wall visibly reacts without breaking. |
| 84–70% | `smoking` | Thin smoke plumes from impact points. No geometry change. |
| 69–40% | `rubble` | Wall chunks removed at impact points, rubble piles at base, interior more exposed |
| 39–1% | `breached` | Full wall gap(s). **Frontline firing bonus becomes active.** Heavy smoke, embers. |
| 0% | `destroyed` | Full implosion animation, castle collapses inward, dust cloud |

**Critical implementation note — breach behavior:** a breach grants the *attacking team a damage multiplier* against that castle. It does **not** allow worms to enter the enemy castle. Worms never leave their own side; turn order is unchanged. This was explicitly clarified in design and is easy to misread from the visual.

### Weapon tier → damage escalation

**Every weapon's first hit on a castle is a minimal-damage shockwave.** No weapon skips this — the difference between tiers is how quickly damage *escalates* from there. There is no state in which a hit produces no visible response.

| Weapon tier | Escalation curve | Damage model |
|---|---|---|
| Starter | Slow — ~3 hits to reach prominent structural damage | Discrete damage on impact |
| Upgraded | Medium — ~2 hits to reach prominent structural damage | Discrete damage on impact |
| Rare / Super | Immediate — prominent damage by hit 1–2 | **Sustained/damage-over-time** — applies a smaller per-turn tick that continues across subsequent turns (the "laser saw" model), *in addition to* impact damage |

Implement escalation as a **per-weapon damage multiplier curve indexed by hit count against that castle**, e.g. starter = `[0.3, 0.6, 1.0, 1.0, …]`, upgraded = `[0.4, 1.0, 1.0, …]`, rare = `[1.0, 1.0, …]` `[TUNE]`. Multiply into the damage formula below. Hit count is tracked **per castle, not per weapon** — switching weapons mid-siege does not reset a castle's escalation state.

Rare-weapon DoT must persist across turn boundaries and be visually attributable — a small cutting/sparking VFX at the affected wall section, ticking on each turn transition, so the defending player understands why their castle keeps degrading without being hit.

### Damage formula

```
damage = weapon.baseDamage
       × weapon.escalationCurve[castle.hitCount]
       × worm.strengthMultiplier
       × breachBonus
```

- `weapon.escalationCurve[castle.hitCount]` is the ramp described above — always < 1.0 on the first hit, reaching 1.0 at a rate set by weapon tier. Only applies to castle targets; worm damage does not escalate.
- `worm.strengthMultiplier` derives from the worm's strength stat (earned via battle wins, player-allocated) plus equipped accessories.
- `breachBonus` = 1.0 when target castle is `intact`/`smoking`/`rubble`, > 1.0 `[TUNE]` when `breached`.
- Applies identically to worm targets and castle targets.

### Castle HP by upgrade tier `[TUNE]`

| Castle tier | Max HP (starter-weapon hits to destroy) |
|---|---|
| Tier 1 (default) | ~10 |
| Tier 2 | ~16 |
| Tier 3 | ~24 |

---

## Responsive Behavior

Landscape is the only supported orientation. If the device is held in portrait, show a full-screen rotate prompt rather than attempting a portrait layout.

| Breakpoint | Changes |
|---|---|
| Small phone (< 700dp wide) | Control band grows to 33% height. Weapon slots drop to 32dp. Castle width increases to 35% each to preserve castle legibility; courtyard clamps to `spacing-courtyard-min`. Badge sub-lines ("Worm 2 of 3") are hidden — badge shows team name only. |
| Standard phone (700–1000dp) | Reference layout as specified above. |
| Tablet (> 1000dp) | Control band shrinks to 24% height (more battlefield). Controls do **not** scale up proportionally — joystick and fire button stay at 72dp and remain pinned to the bottom corners within thumb reach; they must not drift toward screen center. Castles scale up with the viewport. Weapon tray gains `spacing-md` between slots. |

**Reasoning for the tablet rule:** proportional scaling of controls on a tablet puts the fire button outside comfortable thumb reach and makes the joystick oversized relative to actual thumb travel. Anchor-and-hold is correct here even though it looks "small" in a tablet mockup.

---

## Edge Cases

- **Both castles destroyed on the same turn** (e.g. a splash weapon or simultaneous DoT tick): the team whose castle hit 0 HP *last* wins. If truly simultaneous within the same tick, resolve as a draw and show a draw variant of the victory screen. Do not leave the match hanging.
- **Last worm eliminated but castle still standing:** match continues — the team with worms remaining keeps taking turns to finish the castle, firing unopposed. Turn timer still applies. This is a deliberate consequence of the both-conditions win rule and needs a HUD cue: show "FINISH THE CASTLE" as a persistent banner so the player isn't confused about why the match hasn't ended.
- **Castle destroyed but worms still alive:** mirror of the above — banner reads "ELIMINATE THE TEAM".
- **Worm knocked entirely outside castle bounds:** instant elimination for the remainder of the battle. Play the standard elimination beat (smoke → burst) at the point of departure, not offscreen.
- **All worms eliminated by out-of-bounds knockback simultaneously:** treat as normal elimination; win condition evaluation is unchanged.
- **Turn timer expires mid-drag:** the shot does **not** fire. Turn passes. Cancel the trajectory arc immediately so the player isn't misled into thinking a shot went out.
- **Rare-weapon DoT active on a castle that reaches 0 HP from a direct hit:** cancel remaining DoT ticks, resolve destruction immediately.
- **Player disconnects (online multiplayer):** hold the match for `[TUNE]` seconds with a reconnect banner; on timeout, award the match to the remaining player. Do not silently auto-play their turns.
- **Zero unlocked weapons with ammo remaining:** the fire button stays enabled and the melee/default weapon is always available with infinite uses, so a player can never be softlocked into passing turns forever.
- **Very long team names in customization:** truncate to 12 characters with ellipsis in the turn badge. Full name only appears on the victory/summary screen.
- **8-worm teams (max size):** worm count badge shows `8–8`; HP pills at this density will overlap if worms cluster. Apply vertical stagger to HP pills when two worms are within 40dp horizontally — offset the rear one up by 12dp.
- **Reduced-motion OS setting enabled:** see Accessibility.

---

## Animation / Motion

| Element | Trigger | Animation | Duration | Easing |
|---|---|---|---|---|
| Camera | Turn start | Pan to active worm | 600ms | `ease-in-out` |
| Camera | Fire | Follow projectile | flight duration | linear, damped |
| Camera | Impact | Shake, amplitude scaled to damage | 300ms | `ease-out` |
| `WormSprite` (active) | Idle loop | Vertical bob ±2dp | 2000ms loop | `ease-in-out` |
| `WormSprite` | Takes damage | White flash, then knockback arc | 100ms flash / 400ms arc | `ease-out` |
| `WormSprite` | Eliminated | Smoke builds → comic burst → despawn | 400ms + 300ms | `ease-in` then `ease-out` |
| `FireButton` | Press | Depress 6dp → 2dp shadow | 80ms | `ease-out` |
| `FireButton` | Charging | Radial power meter fills circumference | up to 1500ms | linear |
| `FireButton` | Release | Snap back to 6dp | 120ms | `ease-out-back` |
| `WeaponSlot` | Select | Lift 2dp + glow fade in | 150ms | `ease-out` |
| `TrajectoryArc` | Aiming | Dashes march along path | 800ms loop | linear |
| `CastleHPBar` | HP decrease | Bar drains to new value | 500ms | `ease-out` |
| `CastleHPBar` | Critical | Pulse opacity 1.0 → 0.6 | 800ms loop | `ease-in-out` |
| Castle | Shockwave hit | Ripple expands from impact across wall face + dust puff | 350ms | `ease-out` |
| Castle | Stage change | Cross-fade to next damage stage + debris burst | 400ms | `ease-out` |
| Castle | Destroyed | Implosion: walls collapse inward, dust expands | 1200ms | `ease-in` for collapse, `ease-out` for dust |
| `TurnTimer` | ≤10s | Scale pulse 1.0 → 1.08 → 1.0 | 1000ms loop | `ease-in-out` |

**Haptics** (decided: included):

| Event | Haptic |
|---|---|
| Fire | Medium impact |
| Projectile hits castle (shockwave / early hit) | Medium impact — deliberately lighter than a structural hit, so the escalation is felt as well as seen |
| Projectile hits castle (structural damage) | Heavy impact |
| Projectile hits worm | Heavy impact + short double-tap |
| Worm eliminated | Heavy impact, longer |
| Castle destroyed | Sustained rumble, ~800ms |
| Weapon slot select | Light selection tick |
| Turn timer ≤5s | Light tick each second |

---

## Accessibility Notes

**This is a game screen, so the usual DOM-focus model doesn't apply — but these are still required:**

- **Touch targets:** every interactive element meets a **44 × 44dp minimum**. Weapon slots at 36dp visual must carry a 44dp hit area with `spacing-sm` between hit areas so adjacent slots aren't mis-tapped. The joystick and fire button at 72dp already exceed minimum.
- **Color independence:** team identity must never rely on color alone. Team A and Team B worms need a distinguishing **shape or marker** (e.g. different helmet/accessory silhouette, or a persistent side-indicator) so the ~8% of players with color vision deficiency can tell teams apart. The current purple/orange placeholder pair is distinguishable in most CVD types but this must not be the only signal. Same applies to the two castle HP bars — label them ("A CASTLE" / "B CASTLE") as specified, never color-only.
- **Contrast:** all HUD text on `color-cream` badges against `color-outline` text passes AA comfortably. HP bar fills are decorative but the *numeric* HP should be available — expose an optional "show damage numbers" setting for players who can't read the bar fill reliably.
- **Reduced motion:** when the OS reduced-motion flag is set — disable camera shake entirely, reduce the idle bob to none, replace the castle implosion with a 300ms cross-fade to the destroyed state, and keep the trajectory arc static rather than marching. Do **not** disable the damage-stage changes themselves; they carry game state.
- **Turn timer accessibility:** the ≤10s pulse is a visual-only cue today. Pair it with the per-second haptic tick (already specified) so it's perceivable non-visually.
- **Screen reader:** full VoiceOver/TalkBack gameplay is out of scope for v1, but HUD elements should still carry labels (`"Team A's turn, worm 2 of 3"`, `"24 seconds remaining"`, `"Team B castle, 55 percent"`) so a player using a reader can at least track match state. Weapon slots need labels including their locked state.
- **Text scaling:** HUD badges must accommodate up to 150% system text scale without clipping. Below that they grow; above it, allow the sub-line to drop rather than truncating the primary label.

---

## Open Items for Design

1. **Final numeric balancing** — all `[TUNE]` values (castle HP tiers, escalation curves, breach bonus multiplier, weapon base damage, strength multiplier curve, disconnect grace period) need a playtest pass.
2. **Team color palette** — purple/orange are placeholders pending final art direction; implement as themeable.

> **Resolved:** the earlier "shield cue" question is closed. Under the shockwave model every hit produces visible feedback from the first impact, so no separate protection indicator is needed. `color-shield` is retained in the token table only in case a future ability introduces a real shield.

---

## Related Files

- `worm_battle_concept.md` — full design decision log, including rationale for the win condition, progression, and damage model.
- `worm_battle_mockup_castles.html` — visual reference for this spec (chunky 3D castle battle screen, showing Team B mid-breach).
