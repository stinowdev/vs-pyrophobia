# Changelog

All notable changes to this project will be documented in this file.

Feature (F) and decision (D) numbers refer to [FEATURES.md](FEATURES.md).

## [Unreleased]

### Added

- Initial Pyrophobia project scaffold.
- Universal client and server mod entry point.
- F01: hold right-click with no block targeted to brandish a lit main-hand
  torch (collectible behavior), with third- and first-person raise animations.
  Block-targeted right-clicks keep vanilla placement, relighting, and ignition.
- F02a: brandishing has a periodic chance to make nearby hostiles that are
  targeting the player flee (vanilla AI).
- Initial design baseline for torch, fire, and wildlife interactions.
- Local build and packaging script.
- GitHub Actions build and draft-release workflow.
- Starter project, design, release, and testing documentation.

### Known limitations

- Only brandish scare so far - ground fires and richer reactions are later.
- Aiming at an entity with a lit main-hand torch brandishes instead of
  running right-click entity interactions such as boarding a boat.
- Runtime configuration does not exist yet.
- Dedicated-server brandish->scare sync still needs a playtest confirmation.
