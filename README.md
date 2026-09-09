# Ranching - Chick Addon

A standalone add-on for **original Smoothbrain Ranching**. Adds faster chick maturation based on the nearest player's Ranching level, plus a growth percentage on hover unlocked at a configurable skill level. Eggs and other animals are unaffected.

## Installation

Install with r2modman/Thunderstore; BepInEx and original Ranching are dependencies. For manual installation, place `RanchingChickAddon.dll` in your profile's `BepInEx/plugins/RanchingChickAddon` directory with the dependencies installed.

Use original Ranching 1.1.6, not the earlier experimental 1.1.7 fork with these features built in. The add-on detects that fork and disables itself to avoid applying growth twice. Do not install two copies of Ranching.

Install the add-on and Ranching on every participating client and the host/dedicated server. The peer owning each chick awards the bonus; a host-only installation cannot guarantee acceleration when another client owns the creature. Configuration is synchronized from a modded host/server and locked to admins by default. Each viewer's own Ranching level controls the tooltip.

## Settings

File: `BepInEx/config/com.ziluck.valheim.ranchingchickaddon.cfg`.

| Setting | Default | Meaning |
| --- | --- | --- |
| Lock Configuration | true | Host/server settings are controlled by admins. |
| Chicken Growth Factor | 2 | Speed at skill 100 while the nearest player is within 10 metres; range 1–10. 1 disables further bonus growth. |
| Growth Info Level Requirement | 30 | Skill required to see growth percentage; range 0–100. 0 disables the display. |

At skill 50 with factor 2, growth speed is 1.5x. Proximity is sampled at the game's growth checks, roughly every 10 seconds. No bonus is awarded on the first observation after loading or ownership transfer, and catch-up is capped at one interval. Time away from loaded chicks does not earn proximity bonuses. The display includes already-earned bonus age and preserves any existing hover text.

Bonus age is saved in the chick's network data. Disabling further acceleration retains earned progress; uninstalling stops applying the bonus without altering vanilla birth timestamps. The bonus key is compatible with our earlier experimental fork.

## Build

```powershell
./Build.ps1 -GamePath "E:\Games\SteamLibrary\steamapps\common\Valheim"
```

This compiles Release, runs calculation checks, and creates a validated Thunderstore ZIP under `artifacts`. Set `-OutputDirectory` to choose another destination. Requires PowerShell, a .NET SDK and local Valheim/BepInEx assemblies. No publicized game assemblies are needed, and build scripts never install the mod.

`Package.ps1` validates and packages an existing Release build. The ZIP contains one plugin DLL, required Thunderstore metadata, the icon, and license/attribution files. ServerSync is merged into the DLL under its MIT-0 license. Ranching and SkillManager are not bundled or linked as assembly references; the add-on reads Ranching's synchronized skill value.

## Testing status

Build and automated checks are provided. In-game hover rendering, actual prefab selection and multiplayer behavior still need testing in a disposable world before using a shared save.

## Credits and license

Original add-on code: MIT, copyright Michael Ziluck. Original Ranching is by blaxxun/Smoothbrain and remains a separate dependency under its own terms. The chick artwork is from Valheim; see `ATTRIBUTION.md`. ServerSync's license is included in `THIRD_PARTY_NOTICES.md` and `Libs/ServerSync.LICENSE.txt`.

## Check out my other mods

- [Animal Feed Guard](https://thunderstore.io/c/valheim/p/DocZee/AnimalFeedGuard/) — keeps animal feed on the ground near tamed animals instead of letting automatic pickup collect it.

