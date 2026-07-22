# Pyrophobia design

This document tracks feature status and the design decisions that constrain
future work. User-facing behavior belongs in [README.md](README.md); release
history belongs in [CHANGELOG.md](CHANGELOG.md).

## Feature status

| ID | Status | Feature | Config key | Default | Authority |
|---|---|---|---|---|---|
| F01 | Planned | Raise a main-hand torch to deter hostile animals | - | - | Server |
| F02 | Planned | Fire sources influence nearby animal behavior | - | - | Server |
| F03 | Planned | Animals can flee, disengage, investigate, or ignore fire | - | - | Server |
| F04 | Planned | Configure fire sources, ranges, probabilities, and reactions | - | - | Server |

Planned features do not receive runtime config keys until their behavior is
implemented.

## Locked decisions

| ID | State | Decision |
|---|---|---|
| D01 | Active | The project is a universal code mod with no external mod dependencies. |
| D02 | Active | Release metadata is owned by `resources/modinfo.json`. |
| D03 | Active | The server is authoritative for animal reactions and probability checks. |
| D04 | Active | The first scope is hostile animals targeting or aggressive toward the player. |
| D05 | Planned | A scare probability is evaluated on controlled intervals, not every game tick. |
| D06 | Planned | Fire reactions are configurable per fire source and animal behavior profile. |

## Design notes

Fire is treated as a signal, not a universal fear effect. A torch held up by
the player may be more immediate and threatening than a distant firepit. Some
predatory animals may investigate an isolated fire, creating a configurable
counter-effect rather than a guaranteed safety zone.

The first release should distinguish between:

- fleeing from the player;
- abandoning an active pursuit without fleeing;
- investigating a fire source; and
- remaining unaffected.

## Configuration contract

The scaffold has no runtime configuration yet. Add serialized settings only
when a feature uses them, define ownership and synchronization here, and
document user-facing values in [README.md](README.md).
