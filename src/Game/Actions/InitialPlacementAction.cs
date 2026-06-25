using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TT2026.Game.Behaviors;
using TT2026.Game.Definitions;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.Actions;

public struct InitialPlacementAction : IPlacementAction
{
    private static HashSet<int> _addedBoardSpaces = new();
    public struct Placement
    {
        public int UnitNationId { get; set; }
        public int BoardSpaceId { get; set; }
        public int UnitTypeId { get; set; }
        public int InitialPlacementId { get; set; }
    }

    public int FactionId { get; set; }
    public Placement[] Placements { get; set; }
    public int BoardSpaceId => Placements.Any() ? Placements.Last().BoardSpaceId : -1;

    [JsonIgnore] public IPlayerAction From { get; private set; }
    public IEnumerable<IPlayerAction> Next(GameState gameState)
    {
        Faction faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) yield break;

        var placementBehavior = gameState.GetGameBehavior<PlacementBehavior>();
        if (placementBehavior == null) throw new InvalidOperationException($"Gamestate does not have {typeof(TTSyncronizationBehavior).Name} enabled");
        if (placementBehavior.PlayersPlaced.List.Contains(FactionId)) yield break;

        foreach ((int placementId, Nation nation, var expectedPlacement) in IterateExpectedPlacements(gameState))
        {
            if (Placements.Any(x => x.InitialPlacementId == placementId)) continue;
        
            foreach (var unitType in gameState.GetEntitiesOfType<UnitType>())
            {
                if (!unitType.Definition.Value.MayBePlaced) continue;
                yield return new InitialPlacementAction()
                {
                    FactionId = FactionId,
                    From = this,
                    Placements = Placements.Append(new Placement()
                    {
                        BoardSpaceId = expectedPlacement.BoardSpaceId,
                        UnitTypeId = unitType.ID,
                        UnitNationId = nation.ID,
                        InitialPlacementId = placementId
                    }).ToArray(),
                };
            }
        }

    }

    // To ease validation, we allow any extra illegal placements to pass without issue,
    //  and then simply ignore them during action execution. This should still
    //  effectively prevent cheating while reducing the validation complexity
    // The remaining purpose of validation is to ensure all
    //  required placements are in place, as any missing placements would make
    //  executing the action impossible due to lack of information
    public ActionValidationResult Validate(GameState gameState)
    {
        var phase = gameState.GetGameBehavior<TTSyncronizationBehavior>()?.GetPhaseData();
        if (phase is null) throw new InvalidOperationException($"Gamestate does not have {typeof(TTSyncronizationBehavior).Name} enabled)");
        // Setup is done simultaneously, and Setup has no subphase, so we only need to check the primary phase (Season) == Setup
        if (phase.Value.Season != Season.Setup) return ActionValidationResult.Illegal;
        
        // TODO Implement origin client validation
        //if (FactionId != factionId) return ActionValidationResult.Illegal;
        
        Faction faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) return ActionValidationResult.Illegal;
        
        var placementBehavior = gameState.GetGameBehavior<PlacementBehavior>();
        if (placementBehavior == null) throw new InvalidOperationException($"Gamestate does not have {typeof(TTSyncronizationBehavior).Name} enabled");
        if (placementBehavior.PlayersPlaced.List.Contains(FactionId)) return ActionValidationResult.Illegal;
        
        foreach ((int i, Nation nation, var expectedPlacement) in IterateExpectedPlacements(gameState))
        {
            bool placementFound = false;
            foreach (var placement in Placements.Where(x => x.UnitNationId == nation.ID))
            {
                var unitType = gameState.GetEntity<UnitType>(placement.UnitTypeId);
                if (unitType is null) return ActionValidationResult.Illegal;
                if (!unitType.Definition.Value.MayBePlaced) return ActionValidationResult.Illegal;
                if (placement.InitialPlacementId != i) continue;
                if (placement.BoardSpaceId != expectedPlacement.BoardSpaceId) continue;
                
                // Fail the validation if there are any duplicate valid placements
                // I don't *think* you could cheat by exploiting that... but better
                //  safe than sorry
                if (placementFound) return ActionValidationResult.Illegal;
                
                placementFound = true;
            }
            if (!placementFound) return ActionValidationResult.Incomplete;
        }

        return ActionValidationResult.Valid;
    }

    public void ExecuteOn(ServerGameState gameState)
    {
        foreach ((int i, Nation nation, var expectedPlacement) in IterateExpectedPlacements(gameState))
        {
            foreach (var placement in Placements)
            {
                if (placement.UnitNationId == nation.ID && placement.InitialPlacementId == i)
                {
                    Unit unit = gameState.InstantiateGameEntity<Unit>();
                    unit.UnitTypeId.Value = placement.UnitTypeId;
                    unit.BoardSpaceId.Value = expectedPlacement.BoardSpaceId;
                    unit.Pips.Value = expectedPlacement.NumCadres;
                    unit.NationId.Value = nation.ID;
                    break;
                }
            }
        }
        var placementBehavior = gameState.GetGameBehavior<PlacementBehavior>();
        placementBehavior.PlayersPlaced.List.Add(FactionId);
        placementBehavior.CommitState();
    }

    public string StepDescription(GameState gameState)
    {
        if (Placements is null || !Placements.Any()) return string.Empty;
        
        var placement = Placements.Last();
        var unitType = gameState.GetEntity<UnitType>(placement.UnitTypeId);
        var nation = gameState.GetEntity<Nation>(placement.UnitNationId);
        var boardSpace = gameState.GetEntity<BoardSpace>(placement.BoardSpaceId);
        if (unitType is null || nation is null || boardSpace is null)
            return $"ERR in {nameof(InitialPlacementAction)}";
        try
        {
            int numPips = gameState.GetEntity<UnitPlacement>(placement.InitialPlacementId).StartingPips.Value;
            return $"Place {numPips} {nation.Definition.Value.AdjectiveName} {unitType.Definition.Value.PluralName} in {boardSpace.Name}";
        }
        catch (IndexOutOfRangeException)
        {
            return $"ERR in {nameof(InitialPlacementAction)}";
        }
    }

    public IEnumerable<int> HighlightEntities(GameState gameState)
    {
        foreach (var placement in Placements)
        {
            yield return placement.BoardSpaceId;
        }
    }

    public bool DuplicatesWith(IEnumerable<IPlayerAction> otherActions)
    {
        if (Placements.Length == 0) return false;
        foreach (var action in otherActions)
        {
            if (action is not InitialPlacementAction placementAction) continue;

            var myPlacement = Placements[^1];
            foreach (var otherPlacement in placementAction.Placements)
            {
                if (otherPlacement.BoardSpaceId == myPlacement.BoardSpaceId
                    && otherPlacement.UnitNationId == myPlacement.UnitNationId
                    && otherPlacement.UnitTypeId == myPlacement.UnitTypeId) return true;
            }
        }
        return false;
    }

    private IEnumerable<(int placementId, Nation nation, InitialPlacement expectedPlacement)> IterateExpectedPlacements(GameState gameState)
    {
        foreach (UnitPlacement placement in gameState.GetEntitiesOfType<UnitPlacement>())
        {
            Nation nation = gameState.GetEntity<Nation>(placement.NationId.Value);
            if (nation.FactionId.Value == -1 || nation.FactionId.Value != FactionId) continue;
            yield return (placement.ID, nation, new InitialPlacement()
            {
                BoardSpaceId =  placement.BoardSpaceId.Value,
                NationId = placement.NationId.Value,
                NumCadres = placement.StartingPips.Value,
            });
        }
    }
}

public struct InitialPlacement
{
    public int BoardSpaceId { get; set; }
    public int NationId { get; set; }
    public int NumCadres { get; set; }
}