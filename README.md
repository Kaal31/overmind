# Overmind

An independently written Ruinarch mod by Kaal31, using
[RuinarchModLoader](https://github.com/Xm0x/RuinarchModLoader).
Designed to run alongside [Ruinarch+](https://github.com/Xm0x/RuinarchMods).
This repository is not a fork and contains no Ruinarch+ or game source.

## Status

**0.1.0-preview: compiled preview.** Compiled successfully against the supplied
installed game assemblies and RuinarchModLoader 0.5.0, with no compiler warnings
or errors. All six patched method names exist in those game assemblies (metadata
check). Harmony runtime patching, gameplay, save compatibility and coexistence
with Ruinarch+ still require in-game validation.

To install the compiled preview, extract the ZIP into the game's `Mods` directory
so that `Mods/Overmind/Overmind.dll` and `Mods/Overmind/mod.json` exist. Install
RuinarchModLoader 0.5.0 separately. Start with a disposable world and copies of
saves, following [the gameplay checks](docs/VALIDATION.md).

## First module: traits

| Flaw | Implemented behavior |
| --- | --- |
| Negligent | On eligible work states, a 25% roll adds 50% to their duration. Production, construction and repairs are affected; combat, eating, travel and emergency care are excluded. No material destruction or item-quality change yet. |
| Envious | Seeing a peer in the same settlement with equipment they lack, a Noble class, Mighty/Empowered, or at least three extra Martial Arts levels adds a named -10 opinion modifier. Reassessed at most once per peer per day. Does not stack indefinitely or automatically create crimes. |
| Paranoid | A named -5 distrust modifier toward non-friend peers seen in the same settlement; removed on the next daily observation if they become friends. Negative `AdjustOpinion` events are 25% stronger. Uses the game's Suspicious object reaction without replacing the original Suspicious trait. Does not fabricate crimes. |

Select a living villager and use the existing **Afflict** submenu. The three
new entries cost 30 mana by default and reuse native target validation, including
affliction immunity, Blessed/Temporal protection and protected ground. Removal
uses the existing **Remove Flaw** action and its native cost, chance and rules.
There is no separate mod panel. Traits use English text and fallback game icons.
New entries currently share Afflict metadata and have no separate upgrade tree.

New flaws join the natural flaw pool by default. This affects future trait
selection, not a retroactive random reassignment of existing villagers. Set
`naturalTraits` to false to use only manual applications. No Trigger Flaw action,
upgrade tree or automatic Chaos Orb reward is implemented in this preview.

## Requirements and build

- Your installed copy of Ruinarch, including `Ruinarch_Data/Managed`.
- RuinarchModLoader with its `Ruinarch.Modding.dll`, `Ruinarch.ModContent.dll`
  (including `ModSave`) and `0Harmony.dll`.
- .NET SDK 8 or newer for building.

Using an already built RuinarchModLoader checkout:

```bash
export RUIN_GAME_DIR="/path/to/Ruinarch"
/path/to/RuinarchModLoader/tools/build-mod.sh /path/to/overmind/Overmind "/path/to/Ruinarch/Mods"
```

Alternatively, the portable build helper uses explicit reference paths:

```bash
python tools/build.py --game-managed "/path/to/Ruinarch/Ruinarch_Data/Managed" --loader-dll "/path/to/Ruinarch.Modding.dll" --content-dll "/path/to/Ruinarch.ModContent.dll" --harmony-dll "/path/to/0Harmony.dll"
```

This writes `build/Overmind/Overmind.dll` and `mod.json`; it does not install them.
Before deployment, run the upstream `tools/check-patches.sh` on the compiled DLL
against your installed game, then follow [the gameplay checks](docs/VALIDATION.md).
Copy the built Overmind folder into the game's Mods directory after validation.

## Configuration

`Mods/Overmind/config.json` is written on first launch. Restart after editing it.

```json
{
  "naturalTraits": true,
  "afflictionManaCost": 30,
  "negligenceChancePercent": 25,
  "negligenceExtraDuration": 0.5,
  "envyOpinionPenalty": 10,
  "paranoidOpinionPenalty": 5,
  "paranoidNegativeOpinionMultiplier": 1.25,
  "diagnosticLogging": false
}
```

## Save handling

Custom trait membership and daily observation history are stored in
`ModData/overmind.traits.json` inside the save through ModSave. Custom names are
excluded from the stock trait save lists to avoid missing-name lookup failures
when loading without Overmind. Removing a flaw removes its named opinion
modifiers. Shared trait definitions do not store an owner or mutable villager data.

Without Overmind, its flaws and behaviors are absent. Already saved opinion
modifiers can remain in the base game's relationship data; for a clean removal,
remove the social flaws with Overmind active and save before uninstalling.
Loading/saving without the mod is not guaranteed to preserve mod data for later
reinstallation. Save compatibility must still be checked in-game.

## Development scope

Traits only. Sandbox mode, new monsters, politics, criminal accusations,
wealth calculations and economic simulation are not included.
English localization is the initial fallback; translation can follow.

Implementation targets were checked against the public reconstructed reference
in [RuinarchRE](https://github.com/Xm0x/RuinarchRE). The actual installed game DLLs
remain the authority. Ruinarch and its assets belong to their respective owners.

## License

Overmind's original code is MIT licensed; see [LICENSE](LICENSE).
Dependencies retain their own licenses. No game binaries or assets are included.
