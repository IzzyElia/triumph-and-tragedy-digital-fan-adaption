using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TT2026.Game.Behaviors;
using TT2026.Game.Definitions;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Actions;

/// <summary>
/// The spending production phase (see also 7.22) from Triumph and Tragedy. Each spent
/// Production point is one <see cref="Production"/> — drawing a card, creating a new Cadre,
/// or reinforcing an existing unit. The whole Production Level must be spent for the action
/// to be valid (a partial spend is <see cref="ActionValidationResult.Incomplete"/>).
///
/// Placement legality is driven entirely by <see cref="UnitTypeDefinition"/> properties,
/// never by checking whether a unit "is a fortress":
/// - <see cref="UnitTypeDefinition.AllowPlacementWithoutSupply"/>: when false the target
///   board space must be in supply (a member of <see cref="Faction.Supply"/>).
/// - <see cref="UnitTypeDefinition.AllowPlacementInOccupiedTerritory"/>: when false the
///   board space's owner AND occupier must both be the placing nation; when true only the
///   occupier must be.
/// - <see cref="UnitTypeDefinition.MaxPerTile"/>: a value below 1 means no per-tile limit; a
///   positive value forbids placement once that many units of the type are already present.
/// The not-at-sea check uses <see cref="BoardSpace.IsLand"/>; the engaged-in-battle check
/// uses <see cref="Unit.IsInBattle"/>. Rule variation: any nation belonging to the faction
/// (<see cref="Nation.FactionId"/>) may place its national units.
/// </summary>
public class ProductionAction : IPlayerAction
{
    public int FactionId { get; set; }
    public Production[] Productions { get; set; }
    public int BoardSpaceId => Productions is { Length: > 0 } ? Productions[^1].BoardSpaceId : -1;

    [JsonIgnore] public IPlayerAction From { get; set; }

    public IEnumerable<IPlayerAction> Next(GameState gameState)
    {
        Faction faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) yield break;

        // Stop offering productions once the whole Production Level has been spent.
        if (Productions.Length >= faction.GetProduction()) yield break;

        // Draw a card: one option per deck (Action / Investment).
        foreach (var deck in gameState.GetEntitiesOfType<Deck>())
        {
            yield return Append(new Production
            {
                Kind = ProductionKind.DrawCard,
                DeckId = deck.ID,
                BoardSpaceId = -1,
            });
        }

        // Create a new Cadre: one option per distinct (nation, tile, unit type). Iterating
        //  distinct faction nations / tiles / types is itself the dedup — no two children
        //  share a (nation, tile, unit type). IsCreateLegal does all the filtering.
        foreach (var nation in faction.GetControlledNations())
            foreach (var boardSpace in gameState.GetEntitiesOfType<BoardSpace>())
                foreach (var unitType in gameState.GetEntitiesOfType<UnitType>())
                {
                    if (!IsCreateLegal(gameState, faction, nation, boardSpace, unitType)) continue;
                    yield return Append(new Production
                    {
                        Kind = ProductionKind.CreateUnit,
                        NationId = nation.ID,
                        UnitTypeId = unitType.ID,
                        BoardSpaceId = boardSpace.ID,
                    });
                }

        // Reinforce an existing unit: one option per eligible unit not already reinforced
        //  earlier in this action (no more than one step per cadre per Production, §7.23).
        foreach (var unit in gameState.GetEntitiesOfType<Unit>())
        {
            if (!IsReinforceLegal(gameState, faction, unit)) continue;
            if (Productions.Any(p => p.Kind == ProductionKind.Reinforce && p.UnitId == unit.ID)) continue;
            yield return Append(new Production
            {
                Kind = ProductionKind.Reinforce,
                UnitId = unit.ID,
                BoardSpaceId = unit.BoardSpaceId.Value,
            });
        }
    }

    private ProductionAction Append(Production production) => new ProductionAction
    {
        FactionId = FactionId,
        From = this,
        Productions = Productions.Append(production).ToArray(),
    };

    public ActionValidationResult Validate(GameState gameState)
    {
        var phase = gameState.GetGameBehavior<TTSyncronizationBehavior>()?.GetPhaseData();
        if (phase is null || phase.Value.Subphase != Subphase.Production) return ActionValidationResult.Illegal;

        var faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) return ActionValidationResult.Illegal;

        foreach (var production in Productions)
            if (!IsProductionLegal(gameState, faction, production)) return ActionValidationResult.Illegal;

        int budget = faction.GetProduction();
        if (Productions.Length > budget) return ActionValidationResult.Illegal;
        // The full Production Level must be spent before the action may be executed.
        if (Productions.Length < budget) return ActionValidationResult.Incomplete;
        return ActionValidationResult.Valid;
    }

    public void ExecuteOn(ServerGameState gameState)
    {
        var faction = gameState.GetEntity<Faction>(FactionId);
        if (faction == null) return;

        // Re-check each production and skip any that is no longer legal, mirroring
        //  InitialPlacementAction's lenient execution.
        foreach (var production in Productions)
        {
            if (!IsProductionLegal(gameState, faction, production)) continue;
            switch (production.Kind)
            {
                case ProductionKind.DrawCard:
                    gameState.GetEntity<Deck>(production.DeckId)?.TryDrawCard(faction);
                    break;
                case ProductionKind.CreateUnit:
                    Unit unit = gameState.InstantiateGameEntity<Unit>();
                    unit.UnitTypeId.Value = production.UnitTypeId;
                    unit.BoardSpaceId.Value = production.BoardSpaceId;
                    unit.NationId.Value = production.NationId;
                    unit.Pips.Value = 1;
                    unit.CommitState();
                    break;
                case ProductionKind.Reinforce:
                    Unit existing = gameState.GetEntity<Unit>(production.UnitId);
                    existing.Pips.Value++;
                    existing.CommitState();
                    break;
            }
        }
        // No phase advancement: there is no ProductionBehavior to mark the faction done yet.
    }

    public string StepDescription(GameState gameState)
    {
        if (Productions is not { Length: > 0 }) return string.Empty;
        var production = Productions[^1];
        switch (production.Kind)
        {
            case ProductionKind.DrawCard:
                return "Draw a card";
            case ProductionKind.CreateUnit:
            {
                var unitType = gameState.GetEntity<UnitType>(production.UnitTypeId);
                var boardSpace = gameState.GetEntity<BoardSpace>(production.BoardSpaceId);
                if (unitType is null || boardSpace is null) return $"ERR in {nameof(ProductionAction)}";
                return $"Build {unitType.Definition.Value.Name} in {boardSpace.Name}";
            }
            case ProductionKind.Reinforce:
            {
                var unit = gameState.GetEntity<Unit>(production.UnitId);
                var unitType = unit is null ? null : gameState.GetEntity<UnitType>(unit.UnitTypeId.Value);
                if (unitType is null) return $"ERR in {nameof(ProductionAction)}";
                return $"Reinforce {unitType.Definition.Value.Name}";
            }
            default:
                return string.Empty;
        }
    }

    public IEnumerable<int> HighlightEntities(GameState gameState)
    {
        if (Productions?.Length > 0 && Productions[^1].BoardSpaceId != -1)
            yield return Productions[^1].BoardSpaceId;
    }

    // Dispatches an individual production to the legality check for its kind.
    private bool IsProductionLegal(GameState gameState, Faction faction, Production production)
    {
        switch (production.Kind)
        {
            case ProductionKind.DrawCard:
                return gameState.GetEntity<Deck>(production.DeckId) != null;
            case ProductionKind.CreateUnit:
            {
                var nation = gameState.GetEntity<Nation>(production.NationId);
                var boardSpace = gameState.GetEntity<BoardSpace>(production.BoardSpaceId);
                var unitType = gameState.GetEntity<UnitType>(production.UnitTypeId);
                if (nation is null || boardSpace is null || unitType is null) return false;
                return IsCreateLegal(gameState, faction, nation, boardSpace, unitType);
            }
            case ProductionKind.Reinforce:
            {
                var unit = gameState.GetEntity<Unit>(production.UnitId);
                if (unit is null) return false;
                return IsReinforceLegal(gameState, faction, unit);
            }
            default:
                return false;
        }
    }

    private bool IsCreateLegal(GameState gameState, Faction faction, Nation nation, BoardSpace boardSpace, UnitType unitType)
    {
        // The placing nation must belong to this faction (any faction nation may place its
        //  own national units, per the rule variation).
        if (nation.FactionId.Value != FactionId) return false;

        var def = unitType.Definition.Value;
        if (!def.MayBePlaced) return false;

        // Units cannot be built at Sea (§7.23).
        if (!boardSpace.IsLand()) return false;

        // Supply: unless the type is exempt, the tile must be in the faction's supply.
        if (!def.AllowPlacementWithoutSupply && !faction.Supply.List.Contains(boardSpace.ID)) return false;

        // Occupied-territory rule. The placing nation must always occupy the tile; when
        //  placement in occupied territory isn't allowed it must own the tile too.
        if (boardSpace.OccupierNation != nation) return false;
        if (!def.AllowPlacementInOccupiedTerritory && boardSpace.OwnerNation != nation) return false;

        // Per-tile limit: count existing units of this type on the tile plus pending
        //  same-type creations queued earlier in this action.
        if (def.MaxPerTile >= 1)
        {
            int present = gameState.GetEntitiesOfType<Unit>()
                .Count(u => u.BoardSpaceId.Value == boardSpace.ID && u.UnitTypeId.Value == unitType.ID);
            present += Productions.Count(p => p.Kind == ProductionKind.CreateUnit
                                              && p.BoardSpaceId == boardSpace.ID
                                              && p.UnitTypeId == unitType.ID);
            if (present >= def.MaxPerTile) return false;
        }

        return true;
    }

    private bool IsReinforceLegal(GameState gameState, Faction faction, Unit unit)
    {
        if (unit.NationId.Value == -1) return false;
        var nation = gameState.GetEntity<Nation>(unit.NationId.Value);
        if (nation is null || nation.FactionId.Value != FactionId) return false;

        var unitType = unit.UnitType;
        if (unitType is null) return false;

        // CV cap (§3.12): a unit cannot be reinforced past its nation's max pips.
        if (unit.Pips.Value >= nation.Definition.Value.MaxUnitPips) return false;

        // Cannot add steps to a unit Engaged in Battle (§7.23).
        if (unit.IsInBattle()) return false;

        var boardSpace = gameState.GetEntity<BoardSpace>(unit.BoardSpaceId.Value);
        if (boardSpace is null || !boardSpace.IsLand()) return false;

        if (!unitType.Definition.Value.AllowPlacementWithoutSupply && !faction.Supply.List.Contains(boardSpace.ID))
            return false;

        return true;
    }
}

public enum ProductionKind
{
    DrawCard,
    CreateUnit,
    Reinforce,
}

/// <summary>
/// A single spent Production point. Which fields are meaningful depends on <see cref="Kind"/>.
/// Kept as one flat concrete struct (rather than a polymorphic hierarchy) so it round-trips
/// through System.Text.Json — see PlayerActionPacket.
/// </summary>
public struct Production : ISyncable
{
    public ProductionKind Kind { get; set; }
    public int DeckId { get; set; }       // DrawCard
    public int NationId { get; set; }     // CreateUnit
    public int UnitTypeId { get; set; }   // CreateUnit
    public int UnitId { get; set; }       // Reinforce
    public int BoardSpaceId { get; set; } // CreateUnit (target tile) / -1 otherwise
}
