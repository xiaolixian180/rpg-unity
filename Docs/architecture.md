# Hero Quest Architecture

## Goal

Build the game as a set of small, replaceable modules. Early code should stay useful when online features, richer data tables, and production UI arrive later.

## Layers

- **Domain**: Pure C# data and formulas. No scene dependency, easy to test.
- **Config**: Balance tables and tunable values. Starts as ScriptableObject containers and can later be generated from spreadsheets or server data.
- **Runtime Core**: Bootstrap, service registry, and event bus for scene-level wiring.
- **Systems**: Gameplay modules such as combat, dungeon, inventory, pets, PvP, and marketplace.
- **Net**: WebSocket-facing abstractions. Game systems should depend on interfaces, not a concrete transport.

## Expansion Rules

- Add new feature modules under `Assets/HeroQuest/Runtime/Systems/<Feature>`.
- Keep formulas in `Domain` when they can be tested without Unity scenes.
- Keep server protocol details behind `Net` interfaces.
- Prefer config-driven data over hard-coded tables once a system grows beyond a prototype.
