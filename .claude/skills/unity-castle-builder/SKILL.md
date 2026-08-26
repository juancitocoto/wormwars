---
name: unity-castle-builder
description: Build and customize the 3D "open dollhouse" castle for WormWars — the procedural castle shell with three worm battle-station ledges, plus the cosmetic upgrade/IAP catalog (wall skins, tower caps, banners, ledge trim, structure tiers) that reskins it. Use this whenever the user asks to create, adjust, resize, or reskin a 3D castle, add or move a battle station/ledge, add a corner tower or crenellation style, or add a new castle upgrade, cosmetic, skin, or in-app-purchase item — even if they just say "castle", "battle station", or "upgrade" without mentioning Unity or C# explicitly. Also consult this before building any other procedural 3D scenery for WormWars, since it documents the project's no-imported-assets, runtime-generated-geometry convention.
---

# WormWars 3D Castle Builder

WormWars already has a 2D, sprite-based battle screen (`worm_battle_handoff_spec.md`,
`Assets/Scripts/World/CastleView.cs`) built around a "dollhouse cutaway" castle — front wall
missing so the worms and damage inside stay visible. This skill covers the **3D** counterpart:
a castle shell built from plain Unity primitives, meant for a build/customization/shop screen
(and, later, a 3D battle view) rather than replacing the 2D screen.

Read this whole file before touching castle code — it's short by design. Everything it
describes already exists in the repo; extending the castle almost always means editing one of
these files, not creating a parallel system.

## The core idea: three walls, three battle stations

An "open dollhouse castle" has a back wall, a left wall, and a right wall — never a front wall,
because the front is the open side the player looks into. Four corner towers stand at all four
corners regardless (it's the *connecting wall* between the two front towers that's missing, not
the towers themselves).

Each of the three walls that exist gets **exactly one ledge** mounted on its interior face, at
worm height, marked as a `CastleBattleStation`. Three walls, three battle stations — this isn't
a configurable count, it falls directly out of "three walls." If a future request wants more or
fewer stations, that means changing the wall count/shape first (see "Changing the shape" below),
not bolting extra ledges onto existing walls.

## File map

| File | Role |
|---|---|
| `Assets/Scripts/World/Castle3DView.cs` | The builder. `Init()` constructs the whole shell (floor, 3 walls, 4 towers, crenellations, 3 battle-station ledges) from primitives. `ApplyUpgrade()`/`ClearUpgrade()` equip/unequip cosmetics. `RefreshDamageStage()` reacts to `CastleController`'s HP-derived damage stage. |
| `Assets/Scripts/World/CastleBattleStation.cs` | Marker component on each ledge: which wall it's on (`CastleWallSide`), its `SpawnPoint` transform (position + outward-facing rotation), and its current `occupant`. |
| `Assets/Scripts/Utility/ProceduralPrimitive3D.cs` | Spawns and tints primitive cubes/cylinders. The 3D equivalent of `ProceduralSprite.cs`. All castle geometry goes through this — never hand-author a mesh or import model files for castle parts. |
| `Assets/Scripts/Core/CastleUpgradeDefinition.cs` | One purchasable/unlockable cosmetic. Pure data — no store or purchase-flow logic. |
| `Assets/Scripts/Core/CastleUpgradeCatalog.cs` | A list of `CastleUpgradeDefinition`s. `CastleUpgradeCatalog.Default()` builds a starter catalog entirely in code (no `.asset` file needed) so the system is demoable without ever having opened the Unity Editor. |
| `Assets/Scripts/Core/CastleController.cs` | Pre-existing HP/damage-stage logic (shared with the 2D castle). `SetTier(int)` was added here for `StructureTier` upgrades — see below. |
| `Assets/Scripts/Core/Enums.cs` | `CastleWallSide` (Back/Left/Right — no Front) and `CastleUpgradeCategory`. |
| `Assets/Scripts/Editor/CastleBuilderMenu.cs` | `WormWars > Build Preview Castle` / `Build Preview Castle With Upgrades` menu commands — the fastest way to actually look at a castle without wiring up a scene. |

## Conventions to follow (don't deviate without a reason)

- **Everything is procedural, nothing is imported.** The whole repo builds its visuals from
  runtime-generated primitives/sprites rather than imported art (`ProceduralSprite.cs` for 2D,
  `ProceduralPrimitive3D.cs` for 3D) — see `Assets/Scripts/World/CastleView.cs` and
  `Assets/Scripts/Layout/BattleLayoutBuilder.cs` for the established pattern. Keep doing this
  for castle work: new parts are new calls to `ProceduralPrimitive3D.Block`/`.Cylinder`, not new
  `.fbx`/`.obj` imports. This also means there are **no `.meta` files or hand-authored `.asset`
  files anywhere in this repo** (Unity hasn't been opened yet) — don't create ScriptableObject
  `.asset` instances by hand; either let a real Unity Editor session create them via the
  `[CreateAssetMenu]` on `CastleUpgradeDefinition`/`CastleUpgradeCatalog`, or build them in code
  the way `CastleUpgradeCatalog.Default()` does.
- **One namespace.** Every script in this project is `namespace WormWars.Core`, regardless of
  which folder it lives in (`World/`, `Core/`, `Utility/`, `UI/`) — only `Layout`, `Bootstrap`
  break that pattern because they're the top-level composition roots. New castle scripts follow
  the same rule.
- **Reuse `DesignTokens`, don't invent new colors.** `Assets/Scripts/Core/DesignTokens.cs` is a
  transcription of the color table in `worm_battle_handoff_spec.md` and is meant to stay in
  sync with it. Pick from existing tokens (`Stone`, `StoneDark`, `Interior`, `Wood`, `WoodDark`,
  team colors, etc.) for new castle parts. Only add a token if the spec genuinely needs a new
  documented one — don't hardcode a hex value inline.
- **Avoid parenting geometry to a scaled primitive.** `ProceduralPrimitive3D.Block/.Cylinder`
  set `localScale` directly on the primitive's own transform. Parenting *another* object to that
  transform inherits its (often non-uniform) scale and distorts the child. Every builder method
  in `Castle3DView` parents new objects to an unscaled container transform (`_wallsRoot`,
  `_ledgesRoot`, etc.) instead — follow that pattern for anything new.
- **`Init()`, not `Awake()`, does the building.** Dimension fields (`width`, `depth`,
  `merlonsPerWall`, ...) need to be set by the caller *before* geometry is built, but
  `AddComponent<Castle3DView>()` runs `Awake()` synchronously — before the caller gets a chance
  to touch those fields. So construction lives in an explicit `Init()` the caller invokes after
  configuring the component (same reasoning as `CastleView.Init()`).

## Adding a new castle upgrade / IAP item

Upgrades are cosmetic-only data (`CastleUpgradeDefinition`) grouped into independent **category
slots** (`CastleUpgradeCategory`: `WallSkin`, `TowerCaps`, `Banner`, `LedgeTrim`,
`StructureTier`). A castle has at most one equipped upgrade *per category* at a time — buying
"Gold Tower Caps" never undoes an already-equipped "Slate Walls," because `Castle3DView` tracks
`_equipped` as a dictionary keyed by category and `Rebuild()` re-derives every visual from that
whole set. Keep this property when extending the system: never make a new upgrade silently reset
a different category's slot.

To add a new upgrade:

1. Decide its `CastleUpgradeCategory`. If it doesn't fit `WallSkin`/`TowerCaps`/`Banner`/
   `LedgeTrim`/`StructureTier`, that's a sign the category enum itself needs a new case — add it
   to `Enums.cs`, then add the fields it needs to `CastleUpgradeDefinition`, then teach
   `Castle3DView.Rebuild()` to read them (following how `wallSkin`/`towerCaps`/`banner`/
   `ledgeTrim` are already read there).
2. Add an entry to `CastleUpgradeCatalog.Default()` (or, once real designer-authored assets are
   in play, a new `.asset` created via `Create > WormWars > Castle Upgrade` in the Editor).
   `storeProductId`/`priceUsd` are placeholders for real IAP/store wiring — set something
   reasonable but don't invent real store integration here; that's a separate system.
3. **`StructureTier` is special**: it's the one category that isn't purely cosmetic. Equipping a
   `StructureTier` upgrade calls `CastleController.SetTier(int)`, which changes `maxHP` per the
   Castle HP table in `worm_battle_handoff_spec.md` (Tier 1 = 10, Tier 2 = 16, Tier 3 = 24) and
   fully heals — this is meant to run in a build/shop screen between battles, not mid-siege. If
   you add a Tier 4+, extend `CastleController.MaxHPForTier`, not just the catalog entry.
4. Preview it: `WormWars > Build Preview Castle With Upgrades` in the Unity Editor menu applies
   every non-`StructureTier` upgrade in the default catalog at once so you can eyeball everything
   together without writing a shop UI.

## Changing the shape (size, wall count, tower style, crenellations)

- **Resize**: `width`/`depth`/`height`/`wallThickness` are public fields on `Castle3DView` —
  every other measurement (wall length, merlon spacing, ledge position) is computed from them,
  so resizing is just setting those fields before `Init()`.
- **More/fewer merlons**: `merlonsPerWall`/`merlonSize`, same idea.
- **A genuinely different shape** (e.g., a round keep, a fourth wall for a fully-enclosed
  variant): don't hack it into `Castle3DView`'s fixed Back/Left/Right assumption. Treat it as a
  new castle *variant* — most likely a new `BuildWalls()`-equivalent method or a new class that
  still uses `ProceduralPrimitive3D` and still produces exactly one `CastleBattleStation` per
  usable interior wall, since "one battle station per wall" is the rule the rest of the system
  (worm placement, the three-walls framing above) is built around. Don't ship a castle variant
  with zero battle stations or with stations that don't line up with a real wall.

## Relationship to the 2D battle screen

`Castle3DView` and `CastleView` (2D) both key off the same `CastleController` for HP and damage
stage — they're two renderers of the same data, not two competing systems. If a change affects
how damage stages *look*, decide whether it belongs in one view or both; don't assume the 2D
spec's exact visual language (hard shadows, specific overlay alphas) needs to be replicated
pixel-for-pixel in 3D, but do keep the same *meaning* per stage (Intact → Shockwave → Smoking →
Rubble → Breached → Destroyed only ever moves forward, matches `CastleController.Stage`, and
`Destroyed` means the walls come down).
