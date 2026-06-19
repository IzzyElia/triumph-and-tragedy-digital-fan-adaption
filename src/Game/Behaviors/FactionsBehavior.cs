using System.Collections.Generic;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.Behaviors;

public class FactionsBehavior : TTGameBehavior
{
    public override void OnGameStart(IGameStartInfo uncastGameStartInfo)
    {
        var startInfo = (GameStartInfo)uncastGameStartInfo;
        var serverGameState = (ServerGameState)GameState;
        foreach (var factionInfo in startInfo.Factions)
        {
            var faction = serverGameState.InstantiateGameEntity<Faction>();
            var leader = GameState.GetEntity<Nation>(factionInfo.Leader);
            foreach (var allyId in factionInfo.Allies)
            {
                var ally = serverGameState.GetEntity<Nation>(allyId);
            }
        }
    }

    public override void OnPhaseTickerAdvancing()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<IPlayerAction> GetPotentialActions(int playerId)
    {
        throw new System.NotImplementedException();
    }
}