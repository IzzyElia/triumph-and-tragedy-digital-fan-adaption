using System;
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
            if (leader is null) throw new InvalidOperationException();
            leader.FactionId.Value = faction.ID;
            leader.IsFactionMajorPower.Value = true;
            leader.CommitState();
            
            foreach (var allyId in factionInfo.Allies)
            {
                var ally = serverGameState.GetEntity<Nation>(allyId);
                ally.FactionId.Value = faction.ID;
                ally.IsFactionMajorPower.Value = true;
                ally.CommitState();
            }
        }
    }

    public override void OnPhaseTickerAdvancing()
    {
        
    }

    public override IEnumerable<IPlayerAction> GetPotentialActions(int playerId)
    {
        yield break;
    }
}