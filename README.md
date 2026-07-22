# Pyrophobia

Pyrophobia is a Vintage Story mod about the relationship between fire and
wildlife: raise a torch to deter hostile animals, while torches and fires in
the world influence how nearby animals behave.

This repository is an early project baseline. The mod currently contains only
its universal entry point and build/package scaffolding. The gameplay contract
is being designed before implementation.

## Planned direction

- Hold a torch in the main hand and hold LMB to raise it defensively.
- Give hostile animals a configurable chance to flee or abandon pursuit.
- Let ground torches, firepits, and other fire sources create environmental
  signals.
- Support animals that investigate or are attracted to isolated fires instead
  of being deterred by them.
- Make fire source strength, animal reactions, distances, and probabilities
  configurable.

The first implementation will focus on hostile animals already targeting or
aggressive toward the player. Other creatures and more elaborate fire behavior
can be added later.

## Compatibility

- Targets Vintage Story **1.22.3** and .NET 10.
- Loads on both the client and server.
- Has no required dependencies beyond the game.

Compatibility is initially tied to the installed game version. Other game
versions remain unverified until they pass the release test matrix.

## Features and design

- [FEATURES.md](FEATURES.md) tracks feature status and locked decisions.
- [CHANGELOG.md](CHANGELOG.md) records release changes and known limitations.
- [docs/TESTING.md](docs/TESTING.md) defines the release regression matrix.
- [docs/MODDB.html](docs/MODDB.html) contains the current Mod DB description.

## Building

`resources/modinfo.json` is the source of truth for release metadata.

```powershell
dotnet build
./build.ps1
./build.ps1 -Deploy
```

The build script creates `Releases/pyrophobia_<version>.zip`. `-Deploy` also
copies that package into the active Vintage Story `Mods` directory.

## License

See [LICENSE](LICENSE). Personal non-commercial use and pull requests back to
the original project are allowed. Redistribution and modpacks require prior
written permission.

### Support

You can support Pyrophobia and other projects on [Patreon](https://patreon.com/stinow).
