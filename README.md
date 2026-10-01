# RanchingChickAddon

**2.x targets Valheim 1.0**, built and checked against 1.0.16. Use the 1.x releases for Ashlands.

A standalone add-on for **original Smoothbrain Ranching**. Adds faster chick maturation based on the nearest player's Ranching level, plus a growth percentage on hover unlocked at a configurable skill level. Eggs and other animals are unaffected.

## Installation

Install with r2modman/Thunderstore; BepInEx and original Ranching are dependencies. For manual installation, place `RanchingChickAddon.dll` in your profile's `BepInEx/plugins/RanchingChickAddon` directory with the dependencies installed.

Use original Ranching 1.1.6 or later, not the earlier experimental 1.1.7 fork with these features built in. The add-on detects that fork and disables itself to avoid applying growth twice. Do not install two copies of Ranching.

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
./ci/Build.ps1 -GamePath "E:\Games\SteamLibrary\steamapps\common\Valheim"
```

This compiles Release, runs calculation checks, and creates a validated Thunderstore ZIP under `artifacts`. Set `-OutputDirectory` to choose another destination. Requires PowerShell, a .NET SDK and local Valheim/BepInEx assemblies. No publicized game assemblies are needed, and build scripts never install the mod.

The build automatically downloads ServerSync v1.20 into an ignored dependency cache and verifies its SHA-256 checksum against `ci/dependencies.json`. This also happens when running `dotnet build` directly; PowerShell 7 (`pwsh`) and internet access are needed on the first build.

`ci/Package.ps1` validates and packages an existing Release build. The ZIP contains one plugin DLL, required Thunderstore metadata, the icon, and license/attribution files. ServerSync is merged into the DLL under its MIT-0 license. Ranching and SkillManager are not bundled or linked as assembly references; the add-on reads Ranching's synchronized skill value.

## Testing status

Valheim 1.0 gameplay was confirmed by the maintainer. Builds run calculation and game/framework reference checks, including the bundled ServerSync library. Full multiplayer regression testing remains separate from these checks.

## Credits and license

Original add-on code: MIT, copyright Michael Ziluck. Original Ranching is by blaxxun/Smoothbrain and remains a separate dependency under its own terms. The chick artwork is from Valheim; see `ATTRIBUTION.md`. ServerSync's license is included in `THIRD_PARTY_NOTICES.md` and `Libs/ServerSync.LICENSE.txt`.

If you'd like to support ongoing modding work, [Ko-fi](https://ko-fi.com/doczee) is available.

## Check out my other mods

- [Hen Egg Pickup](https://thunderstore.io/c/valheim/p/DocZee/HenEggPickup/) — automatically collects chicken eggs once enough adult hens are nearby.

- [Animal Feed Guard](https://thunderstore.io/c/valheim/p/DocZee/AnimalFeedGuard/) — keeps animal feed on the ground near tamed animals instead of letting automatic pickup collect it.


## Automated builds and releases

See [ci/README.md](ci/README.md) for GitHub Actions builds, versioned releases, and automatic publishing to Thunderstore and Hexium. Builds run on each commit to `main`; Hexium publishing is disabled pending team approval.
