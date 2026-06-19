using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;

namespace TT2026.Game.Behaviors;

public abstract class TTGameBehavior : GameBehavior
{
    protected TTSyncronizationBehavior GetSyncronizationBehavior() => GameState.GetGameBehavior<TTSyncronizationBehavior>();

    protected Faction GetPhasingFaction() => GetSyncronizationBehavior().PhasingFaction.Value == -1
        ? null
        : GameState.GetEntity<Faction>(GetSyncronizationBehavior().PhasingFaction.Value);
}