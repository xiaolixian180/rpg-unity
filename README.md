# Hero Quest 2D RPG

Hero Quest is a Unity 2022 2D RPG prototype. The current repository focuses on an extensible foundation first: domain rules, configuration boundaries, runtime services, and testable formulas are separated so gameplay systems can be filled in incrementally.

## Current Scope

- Unity project baseline for a 2D RPG.
- Core runtime bootstrap and service registry.
- Character stat and combat formula models.
- Dungeon access rules and layer configuration placeholders.
- Network connection abstraction reserved for future WebSocket integration.
- EditMode tests for early formula/rule validation.

## Project Layout

```text
Assets/HeroQuest/
  Runtime/
    Core/       Bootstrap, service registry, event bus
    Config/     ScriptableObject balance/config containers
    Domain/     Shared RPG data models and formulas
    Net/        Multiplayer connection abstractions
    Player/     Player profile model
    Systems/    Gameplay system modules
  Tests/
    EditMode/   Lightweight domain tests
Docs/
  architecture.md
  roadmap.md
```

## Next Milestones

1. Replace placeholder runtime wiring with real scene bootstrap objects.
2. Convert product tables into ScriptableObject or JSON-driven data.
3. Implement local single-player dungeon loop before WebSocket multiplayer.
4. Add inventory, equipment, pets, PvP, and trading as separate modules.
