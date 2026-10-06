# Ranching / Asksvin source audit

Audited 2026-10-05 before implementation. This is a static source/assembly/asset audit, not an in-game test.

## Sources

- Original [Ranching.cs at e39822fcf9f2f5eb2f60ccf0db1aabfd514830b9](https://github.com/blaxxun-boop/Ranching/blob/e39822fcf9f2f5eb2f60ccf0db1aabfd514830b9/Ranching/Ranching.cs), upstream `master`, version 1.1.9.
- Installed Gale `UpdatedDefault` Ranching.dll: decompiled plugin version 1.1.9, same generic patch targets.
- Installed Valheim 1.0.16 `assembly_valheim.dll`, SHA-256 `96CFC004F7F4A6F30D070BEF39EAFD79C466A137121C4665A2F19FB9C15C6127`.
- Installed prefab data in the `c4210710` SoftRef asset bundle, read without modifying the game.

The local Ranching fork is an earlier experimental version containing chicken growth changes. It was not treated as upstream or bundled in this addon.

## Findings

Ranching's `TameFaster`, `SetTamingFlag` / `DoNotAlert`, `TamedCreatureDied`, `DisplayInformation`, `StableProcreatingTimes`, and `IncreaseLevel` patches are component-based. None filters for chickens or excludes Asksvin. Adult Asksvin has `Tameable`, `MonsterAI`, `CharacterDrop`, and `Procreation`, so these paths apply directly. Its offspring is `AsksvinEgg`; vanilla birth/hatching/maturation propagate the offspring level.

Base hunger/pregnancy/calming/offspring bonuses use Ranching's settings and skill requirements. The offspring level-up patch uses Ranching's existing maximum-level integration. This addon adds no second XP or drop multiplier and introduces no extra breeding/egg production bonus.

Both `Chicken -> Hen` and `Asksvin_hatchling -> Asksvin` have `Growup`, with a vanilla maturation time of 3000 seconds. Their alternate grown-prefab lists are empty. Both juveniles lack `Tameable`, so adult hunger/breeding hover information is not a missing juvenile feature. `Character.GetHoverText` returns an empty string without `Tameable`; the addon supplies a name and growth percentage at the configured skill level.

Upstream has no `Growup` or `EggGrow` patches. The addon previously covered only chick maturation, not egg incubation. It now covers those two specific juvenile/adult pairs; it still does not modify egg incubation or other species.

## Patch boundaries

- `Growup.GrowUpdate` prefix: owner-only bonus accumulation using the nearest player's synchronized Ranching skill within 10 metres.
- One `BaseAI.GetTimeSinceSpawned` call in that method is replaced with effective age. Vanilla ownership, adult-prefab selection, tameness, level inheritance, and destruction remain intact. Unsupported age-check IL fails patching and disables the addon.
- `Character.GetHoverText` postfix: read-only growth display gated by the local viewer's skill; preserves other hover text.
- No patches added to Ranching itself. No unlicensed upstream implementation copied.
