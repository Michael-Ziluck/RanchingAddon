# RanchingAddon

A companion to [Smoothbrain Ranching](https://thunderstore.io/c/valheim/p/Smoothbrain/Ranching/) for chickens and Asksvin. Adds skill-scaled juvenile growth and growth percentages on hover, while original Ranching supplies taming, drops, XP, and breeding benefits for both species.

Version 3.0.0 targets Valheim 1.0 and replaces RanchingChickAddon. This release is prepared for testing; automatic publication is disabled.

## Installation and upgrading

Install BepInExPack Valheim 5.4.2350 and **original Ranching 1.1.9 or later**. In Gale, use a separate test profile and import the local ZIP. For manual installation, extract `BepInEx/plugins/RanchingAddon/RanchingAddon.dll` into the profile.

**Remove RanchingChickAddon (including the older Ranching_Chick_Addon package) before installing RanchingAddon.** They share a plugin identity to preserve existing configuration; installing both packages leaves duplicate DLLs. Do not use the earlier experimental Ranching fork, which already contains chicken growth patches. RanchingAddon detects that fork and disables itself.

The package, assembly, and display name are now `RanchingAddon`. The plugin ID and configuration filename remain `com.ziluck.valheim.ranchingchickaddon` for compatibility. Existing chick settings and saved growth bonus age are retained without modifying birth timestamps or rewriting saves.

Install RanchingAddon and Ranching on **every participating client and the host/dedicated server**. The creature's owning peer awards bonus age; host-only installation cannot guarantee acceleration when another client owns the creature. Settings synchronize from the host and are locked to admins by default. Each viewer's own Ranching skill controls their hover information.

[shudnal ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/) is optional for editing the BepInEx settings in-game.

## Features and settings

File: `BepInEx/config/com.ziluck.valheim.ranchingchickaddon.cfg`.

| Section | Setting | Default | Meaning |
| --- | --- | --- | --- |
| General | Lock Configuration | true | Only host/server admins can change synchronized settings. |
| Chicks | Chicken Growth Factor | 2 | Chick maturation speed at Ranching 100, with a player within 10 metres. Range 1–10; 1 disables new bonus age. |
| Chicks | Growth Info Level Requirement | 30 | Viewer's skill required to see chick growth percentage. Range 0–100; 0 disables it. |
| Asksvin | Asksvin Growth Factor | 2 | Hatchling maturation speed at Ranching 100, with a player within 10 metres. Range 1–10; 1 disables new bonus age. |
| Asksvin | Growth Info Level Requirement | 30 | Viewer's skill required to see hatchling growth percentage. Range 0–100; 0 disables it. |

At Ranching 50 with factor 2, growth speed is 1.5x. The nearest player's synchronized skill is used. Proximity is sampled at the game's growth checks, roughly every 10 seconds. First observation after loading or ownership transfer gives no retroactive bonus; long gaps are capped at one interval. Unloaded time earns no proximity bonus.

Bonus age is persisted in creature network data, separately for each species. Setting a factor to 1 retains earned age; uninstalling stops applying it. Hover text includes earned bonus age and preserves existing text. Eggs, egg incubation, adult animals, and other juvenile species are unaffected. Vanilla maturation continues to inherit tameness and creature level.

## What original Ranching already does for Asksvin

The audit of upstream Ranching 1.1.9 and the installed game found **no chicken-only restriction** in the following patches. RanchingAddon does not apply them a second time.

| Base Ranching behavior | Asksvin coverage |
| --- | --- |
| Taming speed and taming XP | All `Tameable` creatures; nearest player within 10 metres. |
| Calming while taming | All qualifying `MonsterAI` / `Tameable` creatures; base calming-level setting. |
| Increased slaughter drops and Ranching XP | All tamed `CharacterDrop` creatures; nearest player within 50 metres. |
| Time until hungry | Tamed `Tameable` creatures; base food-level setting. |
| Pregnancy / breeding progress | Tamed creatures with `Procreation`, including adult Asksvin; base pregnancy-level setting. |
| Chance of higher-level offspring | All `Procreation` creatures in Ranching 1.1.9; base level requirement/chance settings. |
| Skill XP multiplier and death loss | The shared Ranching skill; unchanged by this addon. |

Upstream does **not** accelerate `Growup` or add juvenile growth hover text. Those are this addon's additions. See [the source audit](docs/ranching-audit.md) for the exact revision and game checks.

## Build and verification

```powershell
./ci/Build.ps1 -GamePath "E:\Games\SteamLibrary\steamapps\common\Valheim"
```

Build compiles Release, runs growth/ownership/species/hover checks, tests the transpiler against actual game IL, verifies game/framework references, and creates `artifacts/RanchingAddon-3.0.0-Thunderstore.zip`. Use `-OutputDirectory` to choose another destination. Scripts never install the mod or edit saves.

Requires PowerShell 7, a .NET SDK, and local Valheim/BepInEx assemblies. ServerSync v1.20 is downloaded into an ignored cache with SHA-256 verification; ILRepack merges it into the plugin under its MIT-0 license. Ranching and SkillManager remain separate dependencies and are neither copied nor linked as assembly references.

Both registry publishing scripts support `-WhatIf`. GitHub Actions builds and CodeQL use real dedicated-server assemblies; see [ci/README.md](ci/README.md). Publication remains disabled for this test release.

## Testing status

The earlier chick addon was tested in Valheim 1.0 by the maintainer. The new Asksvin behavior is **not yet verified in-game**. Automated checks do not execute Unity or validate multiplayer behavior. Use [the test checklist](docs/testing.md) with a disposable character/world before using this version in your shared save.

## Credits and license

Original addon code: MIT, copyright Michael Ziluck. Original Ranching is by blaxxun/Smoothbrain and remains a separate dependency under its own terms. The chick artwork is from Valheim; see `ATTRIBUTION.md`. ServerSync's license is included in `THIRD_PARTY_NOTICES.md` and `Libs/ServerSync.LICENSE.txt`.

If you'd like to support ongoing modding work, [Ko-fi](https://ko-fi.com/doczee) is available.

## Check out my other mods

- [HenEggPickup](https://thunderstore.io/c/valheim/p/DocZee/HenEggPickup/) — automatically collects chicken eggs once enough adult hens are nearby.
- [AnimalFeedGuard](https://thunderstore.io/c/valheim/p/DocZee/AnimalFeedGuard/) — keeps animal feed on the ground near tamed animals instead of letting automatic pickup collect it.
