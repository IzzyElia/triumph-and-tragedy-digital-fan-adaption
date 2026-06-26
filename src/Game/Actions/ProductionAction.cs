using System.Collections.Generic;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Actions;

/// <summary>
/// The spending production phase (see also 7.22) from Triumph and Tragedy.
/// See UnitType.AllowPlacementWithoutSupply (There is no supply check yet, but use Faction.Supply as a list of board space id's that are in supply from the faction),
/// UnitType.AllowPlacementInOccupiedTerritory (If false, the board space's nation and occupier must both be the nation of the unit to be placed. If true, only the occupier matters)
/// UnitType.MaxPerTile (any value below 1 (0, -1) means no placement limits based on what's already there, a positive number means to disallow placement if that many units of the type are already present)
/// The above 3 combined cover fortress placement rules. That is, *do not* check whether the unit type is literally a fortress (or anything else), just check the UnitType's properties to see what its placement rules are.
/// For the not-at-sea check use BoardSpace.IsLand()
/// For the engaged-in-battle check use Unit.IsInBattle()
/// Rule Variation: Any nation marked as part of the faction (Nation.FactionId) may place their national units there
/// </summary>
public class ProductionAction : IPlayerAction
{
    public int FactionId;
    public IProductionUse[] Productions { get; set; }
    public int BoardSpaceId => Productions is { Length: > 0 } ? Productions[^1].BoardSpaceId : -1;

    public IPlayerAction From { get; set; }
    public IEnumerable<IPlayerAction> Next(GameState gameState)
    {
        throw new System.NotImplementedException();
    }

    public ActionValidationResult Validate(GameState gameState)
    {
        var faction = gameState.GetEntity<Faction>(FactionId);
        if (Productions.Length > faction.GetProduction()) return ActionValidationResult.Illegal;
        
        throw new System.NotImplementedException();
    }

    public void ExecuteOn(ServerGameState gameState)
    {
        throw new System.NotImplementedException();
    }

    public string StepDescription(GameState gameState)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<int> HighlightEntities(GameState gameState)
    {
        throw new System.NotImplementedException();
    }
}

public interface IProductionUse
{
    public int BoardSpaceId { get; }
}

public struct DrawCardProduction : IProductionUse, ISyncable
{
    public int DeckId;
    public int BoardSpaceId => -1; // No location, drawing card to player hand
}

/// <summary>
/// Spend a production to create a new cadre.
/// New cadre's cannot be reinforced in the same production action
/// </summary>
public struct CreateUnitProduction : IProductionUse, ISyncable
{
    public int BoardSpaceId { get; set; }
    public int NationId { get; set; }
    public int UnitTypeId { get; set; }
}

/// <summary>
/// Spend a production to reinforce a cadre.
/// Can only be done once per cadre per production action
/// </summary>
public struct ReinforceUnitProduction : IProductionUse, ISyncable
{
    public int UnitId { get; set; }
    public int BoardSpaceId { get; set; } // Calculated from the unit position
}