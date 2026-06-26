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
    public struct Placement
    {
        public int UnitNationId { get; set; }
        public int BoardSpaceId { get; set; }
        public int UnitTypeId { get; set; }
        public int InitialPlacementId { get; set; }
    }

    public int FactionId { get; set; }
    public Placement[] Placements { get; set; }
    public int BoardSpaceId => Placements is { Length: > 0 } ? Placements[^1].BoardSpaceId : -1;

    [JsonIgnore] public IPlayerAction From { get; private set; }
    public IEnumerable<IPlayerAction> Next(GameState gameState)
    {
        Faction faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) yield break;

        var placementBehavior = gameState.GetGameBehavior<PlacementBehavior>();
        if (placementBehavior == null) throw new InvalidOperationException($"Gamestate does not have {typeof(PlacementBehavior).Name} enabled");
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
        if (placementBehavior == null) throw new InvalidOperationException($"Gamestate does not have {typeof(PlacementBehavior).Name} enabled");
        if (placementBehavior.PlayersPlaced.List.Contains(FactionId)) return ActionValidationResult.Illegal;

        // Next() only ever appends placements, so any duplicate or bad-typed *matched*
        //  placement is irreparable and must dominate as Illegal regardless of where it
        //  appears. We therefore scan every slot before concluding Incomplete: Illegal
        //  short-circuits, but a merely-unfilled slot only records the fact.
        bool anyIncomplete = false;
        foreach ((int i, Nation nation, var expectedPlacement) in IterateExpectedPlacements(gameState))
        {
            int matchCount = 0;
            foreach (var placement in Placements.Where(x =>
                         x.UnitNationId == nation.ID
                         && x.InitialPlacementId == i
                         && x.BoardSpaceId == expectedPlacement.BoardSpaceId))
            {
                var unitType = gameState.GetEntity<UnitType>(placement.UnitTypeId);
                if (unitType is null) return ActionValidationResult.Illegal;
                if (!unitType.Definition.Value.MayBePlaced) return ActionValidationResult.Illegal;

                matchCount++;
            }

            // Duplicate valid placements for the same slot can't be undone via Next, so
            //  this node and all its descendants are Illegal.
            if (matchCount > 1) return ActionValidationResult.Illegal;
            if (matchCount == 0) anyIncomplete = true;
        }

        return anyIncomplete ? ActionValidationResult.Incomplete : ActionValidationResult.Valid;
    }

    public void ExecuteOn(ServerGameState gameState)
    {
        foreach ((int i, Nation nation, var expectedPlacement) in IterateExpectedPlacements(gameState))
        {
            foreach (var placement in Placements)
            {
                if (placement.UnitNationId == nation.ID
                    && placement.InitialPlacementId == i
                    && placement.BoardSpaceId == expectedPlacement.BoardSpaceId)
                {
                    Unit unit = gameState.InstantiateGameEntity<Unit>();
                    unit.UnitTypeId.Value = placement.UnitTypeId;
                    unit.BoardSpaceId.Value = expectedPlacement.BoardSpaceId;
                    unit.Pips.Value = expectedPlacement.NumCadres;
                    unit.NationId.Value = nation.ID;
                    unit.CommitState();
                    break;
                }
            }
        }
        var placementBehavior = gameState.GetGameBehavior<PlacementBehavior>();
        placementBehavior.PlayersPlaced.List.Add(FactionId);
        placementBehavior.OnFactionPlaced();
        placementBehavior.CommitState();
    }

    public string StepDescription(GameState gameState)
    {
        if (Placements is null || !Placements.Any()) return string.Empty;
        
        var placement = Placements.Last();
        var unitType = gameState.GetEntity<UnitType>(placement.UnitTypeId);
        var nation = gameState.GetEntity<Nation>(placement.UnitNationId);
        var boardSpace = gameState.GetEntity<BoardSpace>(placement.BoardSpaceId);
        var unitPlacement = gameState.GetEntity<UnitPlacement>(placement.InitialPlacementId);
        if (unitType is null || nation is null || boardSpace is null || unitPlacement is null)
            return $"ERR in {nameof(InitialPlacementAction)}";

        int numPips = unitPlacement.StartingPips.Value;
        return $"Place {numPips} {nation.Definition.Value.AdjectiveName} {unitType.Definition.Value.PluralName} in {boardSpace.Name}";
    }

    public IEnumerable<int> HighlightEntities(GameState gameState)
    {
        if (Placements.Length > 0) yield return Placements[^1].BoardSpaceId;
    }

    public bool DuplicatesWith(IEnumerable<IPlayerAction> otherActions)
    {
        if (Placements is not { Length: > 0 }) return false;
        var myPlacement = Placements[^1];
        foreach (var action in otherActions)
        {
            if (action is not InitialPlacementAction placementAction) continue;
            if (placementAction.Placements is not { Length: > 0 }) continue;
            var otherPlacement = placementAction.Placements[^1];
            if (otherPlacement.BoardSpaceId == myPlacement.BoardSpaceId
                && otherPlacement.UnitNationId == myPlacement.UnitNationId
                && otherPlacement.UnitTypeId == myPlacement.UnitTypeId) return
                true;
        }
        return false;
    }

    private IEnumerable<(int placementId, Nation nation, InitialPlacement expectedPlacement)> IterateExpectedPlacements(GameState gameState)
    {
        foreach (UnitPlacement placement in gameState.GetEntitiesOfType<UnitPlacement>())
        {
            Nation nation = gameState.GetEntity<Nation>(placement.NationId.Value);
            if (nation == null) continue;
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