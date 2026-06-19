using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TT2026.Game.Actions;
using TT2026.Game.Definitions;
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
    }

    public override void OnPhaseTickerAdvancing()
    {
    }

    public override IEnumerable<IPlayerAction> GetPotentialActions(int factionId)
    {
        var phase = GetSyncronizationBehavior().GetPhaseData();
        if (phase.Season == Season.Setup)
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