# Release testing

This matrix is the minimum manual verification required before a Pyrophobia
release. Feature-specific checks should be added as gameplay is implemented.
Run the matrix against the Vintage Story version declared in
`resources/modinfo.json`.

## Test environments

- [ ] Singleplayer world
- [ ] Dedicated server with a matching client mod
- [ ] Fresh installation without prior configuration
- [ ] Existing world upgraded from the previous release

## Regression matrix

| Area | Action | Expected result |
|---|---|---|
| Client load | Join a singleplayer or multiplayer world | The client log reports that the mod loaded, with no errors |
| Server load | Start a singleplayer world or dedicated server | The server log reports that the mod loaded, with no errors |
| Reconnect | Leave and rejoin the world | The mod reloads cleanly and does not retain stale static state |
| Compatibility | Run the feature-specific matrix on the declared game version | All documented behavior matches the release contract |

## Package checks

- [ ] `dotnet format Pyrophobia.slnx --verify-no-changes --no-restore` passes.
- [ ] `dotnet build Pyrophobia.csproj -c Release --no-restore` passes without warnings.
- [ ] `./build.ps1` creates the versioned zip from `modinfo.json`.
- [ ] The zip contains `Pyrophobia.dll`, `modinfo.json`, and the `assets` tree.
- [ ] The zip excludes PDB, deps.json, local caches, and game assemblies.
- [ ] The release tag exactly matches `v<modinfo.version>`.
- [ ] The matching `CHANGELOG.md` section is used as the release description.
