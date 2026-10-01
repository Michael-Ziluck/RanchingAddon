# Changelog

## 2.0.1

- Use `RanchingChickAddon` for the README title while retaining the existing Thunderstore listing.
- Move build, package, and publishing scripts into `ci` and normalize `tests/checks`.
- Restore checksum-pinned ServerSync at build time instead of storing the dependency DLL in Git.
- Add repository sponsorship links and automatic dependency update checks.

## 2.0.0

- Formally target Valheim 1.0; compiled and checked against 1.0.16.
- Require BepInExPack Valheim 5.4.2350.
- Add GitHub Actions builds and automatic publication of new versions from main.
- Add Hexium publishing scaffolding, disabled until DocZee is approved.
- Keep plugin IDs and configuration files stable for existing installations.
- Update bundled ServerSync to v1.20, compiled for Valheim 1.0.

## 1.0.0

- Standalone add-on for original Ranching, using plugin ID `com.ziluck.valheim.ranchingchickaddon`.
- Configurable, skill-scaled chick growth within 10 metres of a player.
- Configurable skill requirement for growth percentage on hover.
- Owner-only bonus accumulation and host-synchronized configuration.
- Detect the earlier experimental Ranching fork to avoid duplicate growth effects.
- Include a build-and-package script for Thunderstore.
