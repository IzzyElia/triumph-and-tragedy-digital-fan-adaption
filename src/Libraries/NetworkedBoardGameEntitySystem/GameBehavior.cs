using System.Collections.Generic;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public abstract class GameBehavior : GameEntity
{
    public abstract void OnGameStart(IGameStartInfo gameStartInfo);
    public abstract void OnPhaseTickerAdvancing();
    public abstract IEnumerable<IPlayerAction> GetPotentialActions(int playerId);
}