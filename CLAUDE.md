# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A digital adaptation of the board game *Triumph and Tragedy*, built in **Godot 4.5.1** with **C# / .NET 8** (`net9.0` only for the Android target). Root namespace is `TT2026`. The project is in an early state — several files are works-in-progress and may not compile (e.g. `src/Game/ServerFactory.cs`).

## Building & running

- **Do not run builds yourself — the user compiles in their own environment.** (See memory `never-compile-user-does`.)
- There is **no test project or test framework** in the repo. Don't assume `dotnet test` exists.
- The app runs through the Godot editor/runtime. At runtime, pressing **`1`** opens a debug menu (via `IzzysUI`) to restart the server or create+connect a client — see `GlobalSingletons/GameController._UnhandledKeyInput`.

## Namespace vs. folder conventions

Namespaces do **not** mirror folder casing. Folders are `src/Libraries/...` but namespaces are lowercase `TT2026.libraries.<LibName>` (e.g. `TT2026.libraries.NetworkedBoardGameEntitySystem`, `TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib`). Game code lives under `TT2026.Game`, autoload singletons under `TT2026.GlobalSingletons`. Match the namespace of the surrounding library, not the directory path.

## Core architecture: NetworkedBoardGameEntitySystem

This is the library under active development (`src/Libraries/NetworkedBoardGameEntitySystem`). It handles networking and game-state tracking for online multiplayer board games. The game in `src/Game` is built on top of it.

**Authoritative server, per-client state.** Each client keeps its own `GameState`; the server keeps the master. Because of hidden information, a client's view may legitimately differ from the server's. State flows server → clients only.

- `GameState` (abstract) → `ServerGameState` (`IsServerSide = true`, authoritative) and `ClientGameState`. Each owns a `Mutex` — **all state mutation must happen under `lock(gameState.Mutex)`** because the LiteNetLib network thread and Godot's main thread both touch it.
- `GameEntity` holds a `__SyncedData` dictionary of named `EntityGameData` fields. Concrete data types subclass `EntityGameData` and implement `SerializeData` / `DeserializeDataAndSetStateToIt` / `ValidateData` (string-based). `SyncedInt` is the reference example. An entity registers each field by calling the `EntityGameData` base constructor with a `variableKey`.
- **Full history is retained.** `EntityGameData` stores a list of `DataHistoryFrame(gameStepID, serializedData)`. The game can be reconstructed at any past step via `SetStateToStep(stepId)` (`-1` = live/latest). This is the central design constraint — see below.

### Game steps & the history model

- `GameState.GameStepID` is the monotonic clock for the whole game. The server advances it with `ServerGameState.AdvanceGamePhaseTicker()`, which first calls `CommitState()` on every entity, then increments the step.
- `CommitState()` is **server-only**; it serializes each field and, if changed since the last frame, appends a new `DataHistoryFrame` and pushes an `EntityVariableUpdatePacket`. Re-committing the same step without advancing `GameStepID` throws.
- **Entities are never destroyed** — to "remove" one, flag it as dead via synced data. See the deliberately named `DestroyGameEntity_UNUSED_SEE_DOCS`. Destroying an entity would break history reconstruction.
- New entities come from `ServerGameState.InstantiateGameEntity<T>()` (assigns a monotonic id from `_idTicker`).

### Networking

- `NetworkPeer` (abstract) wraps a LiteNetLib `NetManager`. `Server` and `Client` subclass it. Wire format: everything is JSON — a payload is serialized, wrapped in a `JsonPacket { Type = typeof(payload).Name, Payload = <json> }`, and sent `ReliableUnordered`.
- Receiving dispatches on `JsonPacket.Type` (the type's `.Name`) in `ReceiveJsonPacket`. The client handles `EntityVariableUpdatePacket` and `GameBehaviorSyncPacket`. A `BadPacketException` thrown during handling disconnects the offending peer.
- `NetManager.PollEvents()` is pumped every frame from `GameController._Process`, so peers register themselves in `GameController.NetworkPeers` on construction.
- On a new connection (`Server.OnPeerConnected`), the server replays the **entire history** of every entity field to the joiner via `GenerateSyncPacketsForEntireHistory()`.
- The server listens on port **8080**.

### Type discovery (reflection)

Clients reconstruct entities/behaviors from packets by type name, so concrete `GameEntity` and `GameBehavior` types must be registered. `GameState.LoadTypesFromCurrentAssembly()` reflectively populates the static `KnownEntityTypes` / `KnownBehaviorTypes` keyed by `Type.FullName`. Packets carry `EntityTypeName` (full name); the client looks the type up and `Activator.CreateInstance`s it. **A new entity/behavior won't sync unless these dictionaries are loaded.**

### Behaviors

`GameBehavior` subclasses are server-enabled via `ServerGameState.SetEnabledGameBehaviors(...)`, which broadcasts a `GameBehaviorSyncPacket` of full type names so clients instantiate the matching set. (Game logic in behaviors is mostly unimplemented so far, e.g. `PlacementBehavior`.)

### Rendering

`GameRenderer` attaches to a `GameState` (one per state; attaching a second throws) and exposes an `EntitiesChanged` set that the data layer populates whenever a field's visible value changes. The Godot-side renderer (`TTRenderer`, `BoardSpaceRenderer`) consumes this.

## Supporting libraries (mostly standalone, in `src/Libraries`)

- **LiteNetLib** — third-party reliable-UDP networking. Don't modify; it's a vendored dependency with its own solution.
- **IzzysConsole** — reflection-driven in-game console. Mark methods/properties with `[ConsoleCommand("name", ...)]` (`API/ConsoleCommandAttribute.cs`); static methods are global, instance methods are reached via scoping. Type parsing is pluggable via `IParameterConverter` + `[ParameterConverter]`.
- **IzzysUI** — debug/context-window UI. Add a node inheriting `IzzysUIController` to the scene; call `IzzysUIController.OpenContextWindow(new ContextWindowInfo(...))` to show one.

## Logging

Use the static `Logger` (`src/Logger.cs`), not `GD.Print` directly. `Logger.Log(msg, onlyShowInContext: LoggingContexts.X)` is gated by contexts enabled in `GameController` (`_loggingContexts` export). `LoggingContexts.Always` is unconditional. Network events are logged under `LoggingContexts.Networking`.
