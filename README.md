# Pyrophobia
<img width="400" alt="pyrophobia" src="https://github.com/user-attachments/assets/a823f8cb-4516-4800-8504-9fe242ea0d72" />

Pyrophobia is a universal code Vintage Story mod about the relationship between fire and
wildlife: raise a torch to deter hostile animals, while torches and fires in
the world influence how nearby animals behave.

The mod currently implements the brandish stance and a first animal reaction:
holding right-click with a lit torch raises it, and nearby hostiles that are
targeting you may flee while it is raised.

## Torch brandishing

- Hold right-click with a lit torch in the main hand, aiming at open air or a
  creature, to raise it.
- Aiming at a block keeps every vanilla torch interaction: right-click places
  the torch, holding right-click relights a placed torch, and shift +
  right-click remains the vanilla ignite gesture for firepits and other
  blocks.
- Extinct and burned-out torches are never brandished.

## Planned direction

- Give hostile animals a configurable chance to flee or abandon pursuit.
- Let ground torches, firepits, and other fire sources create environmental
  signals.
- Support animals that investigate or are attracted to isolated fires instead
  of being deterred by them.
- Make fire source strength, animal reactions, distances, and probabilities
  configurable.

The first reaction implementation will focus on hostile animals already
targeting or aggressive toward the player. Other creatures and more elaborate
fire behavior can be added later.

## Compatibility

- Targets Vintage Story **1.22.3** and .NET 10.
- Loads on both the client and server.
- Has no required dependencies beyond the game.

Compatibility is initially tied to the installed game version. Other game
versions remain unverified until they pass the release test matrix.

## Installation

1. Download the latest `pyrophobia_*.zip` from
   [GitHub Releases](https://github.com/stinowdev/vs-pyrophobia/releases/latest).
2. Place the zip in the Vintage Story `Mods` directory.
3. Restart the game, or restart the server and reconnect.

## Building

`resources/modinfo.json` is the source of truth for release metadata.

```powershell
dotnet build
./build.ps1
./build.ps1 -Deploy
```

The build script creates `Releases/pyrophobia_<version>.zip`. `-Deploy` also
copies that package into the active Vintage Story `Mods` directory.

## Documentation

- [FEATURES.md](FEATURES.md) tracks implementation status and design decisions.
- [CHANGELOG.md](CHANGELOG.md) records release changes and known limitations.
- [docs/MODDB.html](docs/MODDB.html) is the maintained Mod DB page copy.
- [docs/TESTING.md](docs/TESTING.md) defines the release regression matrix.

## License

See [LICENSE](LICENSE). Personal non-commercial use and pull requests back to
this repository are allowed. Redistribution and modpacks require prior written
permission.

## Support

You can support Pyrophobia and other projects on
[Ko-fi](https://ko-fi.com/stinow).
