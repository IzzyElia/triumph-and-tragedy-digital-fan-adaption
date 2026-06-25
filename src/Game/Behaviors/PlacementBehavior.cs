using System;
using System.Collections.Generic;
using TT2026.Game.Actions;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Behaviors;

public class PlacementBehavior : TTGameBehavior
{
    public SyncedIntList PlayersPlaced;

    public PlacementBehavior()
    {
        PlayersPlaced = new SyncedIntList(this, nameof(PlayersPlaced));
    }
    public override void OnGameStart(IGameStartInfo gameStartInfo)
    {
        ServerGameState serverGameState = (ServerGameState)GameState;
        PlayersPlaced.List.Clear();
        CommitState();
        
        foreach (var faction in GameState.GetEntitiesOfType<Faction>())
            foreach (var nation in faction.GetControlledNations())
                foreach (var boardSpace in nation.GetControlledBoardSpaces())
                {
                    if (boardSpace.CityType is null) continue;
                    for (int i = 0; i < boardSpace.CityType.Definition.Value.Muster; i++)
                    {
                        var initialPlacement = serverGameState.InstantiateGameEntity<UnitPlacement>();
                        initialPlacement.BoardSpaceId.Value = boardSpace.ID;
                        initialPlacement.NationId.Value = nation.ID;
                        initialPlacement.StartingPips.Value = 1;
                    }
                }
    }

    public override void OnPhaseTickerAdvancing()
    {
        
    }

    public override IEnumerable<IPlayerAction> GetPotentialActions(int factionId)
    {
        var phase = GetSyncronizationBehavior().GetPhaseData();
        if (phase is null) yield break;
        if (phase.Value.Season == Season.Setup)
        {
            InitialPlacementAction rootAction = new InitialPlacementAction()
            {
                FactionId = factionId,
                Placements = Array.Empty<InitialPlacementAction.Placement>()
            };
            foreach (var potentialPlacement in rootAction.Next(GameState))
            {
                yield return potentialPlacement;
            }
        }
    }
}