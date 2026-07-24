# Pyrophobia design

This document tracks feature status and the design decisions that constrain
future work. User-facing behavior belongs in [README.md](README.md); release
history belongs in [CHANGELOG.md](CHANGELOG.md).

## Feature status

| ID | Status | Feature | Config key | Default | Authority |
|---|---|---|---|---|---|
| F01 | Implemented | Hold right-click to brandish a lit main-hand torch | - | - | Universal |
| F02 | Planned | Fire sources, including a brandished torch, influence nearby animal behavior | - | - | Server |
| F03 | Planned | Animals can flee, disengage, investigate, or ignore fire | - | - | Server |
| F04 | Planned | Configure fire sources, ranges, probabilities, and reactions | - | - | Server |

Planned features do not receive runtime config keys until their behavior is
implemented.

## Brandish interaction contract (F01)

| Input | Action |
|---|---|
| Hold right-click with a lit main-hand torch, no block targeted | Raise the torch and hold the stance until released |
| Right-click with a block targeted | Vanilla behavior: place the torch, relight a placed torch, interact |
| Shift + right-click | Vanilla behavior: the ignite gesture for firepits and other blocks |

Vanilla assigns every block-targeted right-click of a lit torch: plain
right-click places it, and the `CanIgnite` behavior owns shift + right-click
as the ignite gesture. Brandishing therefore only begins when no block is
targeted, and once raised the stance holds until release even when the aim
crosses a block.

Extinct and burned-out torches, and torches held by other means than the main
hand, never brandish. The stance eases out on release. The raise animation
plays in third person and, through the `-fp` variant, in first person.

Known limitation: aiming at an entity starts a brandish, so right-click
entity interactions (such as boarding a boat) are consumed while a lit torch
is in the main hand. Scoping the stance to actual threats is F02/F03 work.

## Locked decisions

| ID | State | Decision |
|---|---|---|
| D01 | Active | The project is a universal code mod with no external mod dependencies. |
| D02 | Active | Release metadata is owned by `resources/modinfo.json`. |
| D03 | Active | The server is authoritative for animal reactions and probability checks. |
| D04 | Active | The first scope is hostile animals targeting or aggressive toward the player. |
| D05 | Planned | A scare probability is evaluated on controlled intervals, not every game tick. |
| D06 | Planned | Fire reactions are configurable per fire source and animal behavior profile. |
| D07 | Active | Brandishing uses the interact (right-click) channel and only begins with no block targeted, because vanilla assigns every block-targeted right-click of a lit torch: plain right-click places it and `CanIgnite` owns the shift ignite gesture. Block-targeted input reaches the mod untouched. |
| D08 | Active | F01 is a `CollectibleBehavior` prepended onto lit torches in `AssetsFinalize` (both sides), not Harmony. Must run before `CanIgnite` and use `PreventSubsequent` while raised so aim-crossing a block cannot start fires. Code attach (not a JSON patch) so we can skip extinct / non-torch `BlockTorch` assets. |

## Extension inventory

| Kind | Type | Side | Feature | Notes |
|---|---|---|---|---|
| CollectibleBehavior | `BrandishTorch` | Universal | F01 | Prepended in `AssetsFinalize` on lit `BlockTorch`. Stance is in-memory, not `Attributes`. |
erialized settings only
when a feature uses them, define ownership and synchronization here, and
document user-facing values in [README.md](README.md).
