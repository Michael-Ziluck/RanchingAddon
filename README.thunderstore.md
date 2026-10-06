# RanchingAddon

Faster-growing chicks and Asksvin hatchlings, powered by your **Ranching** skill. See their growth percentage once you reach a configurable skill level.

[Source and build instructions on GitHub](https://github.com/Michael-Ziluck/RanchingAddon)

## Features

- Skill-scaled maturation for chicks **and Asksvin hatchlings** near the nearest player within 10 metres.
- Separate growth-speed settings for chickens and Asksvin. Default: 2x at Ranching 100; 1.5x at Ranching 50.
- Separate skill requirements for growth and egg incubation information on hover. Default: Ranching 30 for both.
- Incubating eggs show percentage and time remaining; stacked and inactive eggs show their status. Egg incubation speed is unchanged.
- Saved bonus growth age, with owner-only updates and no bonus for time spent unloaded.
- Host/server-synchronized configuration, locked to admins by default.

Original Ranching already applies taming speed/XP, calming, increased tamed-creature slaughter drops/XP, hunger information, breeding information, and offspring level bonuses to Asksvin. This addon uses those existing benefits without multiplying them twice. Configure them in Ranching's own settings.

Egg incubation speed, adult animals, and other species are unaffected. Chicken and Asksvin eggs show incubation percentage and time remaining at the corresponding species' hover level, or a stacked/inactive status. Existing chick settings and saved bonus growth are preserved.

## Requirements and upgrading

Requires **original Smoothbrain Ranching 1.1.9 or later** and BepInExPack Valheim 5.4.2350. Install both mods on **every participating client and the host/dedicated server** so creature ownership can move safely between peers.

**Thunderstore users must install Ranching 1.1.9 manually before this addon.** Thunderstore only has Ranching 1.1.6, which this addon cannot load with. Download Ranching 1.1.9 from [Hexium](https://valheim.hexium.gg/mods/Smoothbrain/Ranching) and install its `BepInEx` folder into the same game or mod-manager profile. Keep only one Ranching DLL. The Hexium package installs this dependency automatically.

**RanchingAddon replaces RanchingChickAddon / Ranching_Chick_Addon. Remove the old addon before installing this package.** Do not use the earlier experimental Ranching fork or install duplicate copies of Ranching.

The configuration filename stays `BepInEx/config/com.ziluck.valheim.ranchingchickaddon.cfg` so your existing settings carry over. Configure `Chicken Growth Factor` under `Chicks` and `Asksvin Growth Factor` under `Asksvin` (range 1-10). Each section has its own `Growth Info Level Requirement` (range 0-100; 0 disables growth and egg displays for that species).

## Recommended optional dependency

[shudnal ConfigurationManager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/) lets you edit the settings in-game. It is optional; the mod also works with the configuration file alone.

## Compatibility and testing

Targets Valheim 1.0, built against 1.0.16. The maintainer has tested the current build and confirmed it works as intended. Automated checks cover growth calculations, simulated hooks, and actual game IL. See the [test checklist](https://github.com/Michael-Ziluck/RanchingAddon/blob/main/docs/testing.md).

If you'd like to support ongoing modding work, [Ko-fi](https://ko-fi.com/doczee) is available.

## Check out my other mods

- [HenEggPickup](https://thunderstore.io/c/valheim/p/DocZee/HenEggPickup/) - automatically collects chicken eggs once enough adult hens are nearby.
- [AnimalFeedGuard](https://thunderstore.io/c/valheim/p/DocZee/AnimalFeedGuard/) - keeps animal feed on the ground near tamed animals instead of letting automatic pickup collect it.
