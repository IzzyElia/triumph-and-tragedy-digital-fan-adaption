using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class Faction : GameEntity, ICardHolder
{
    public SyncedIntList Supply;
    public SyncedIntList CardsHeld { get; }

    public Faction()
    {
        Supply = new SyncedIntList(this, nameof(Supply));
        CardsHeld = new SyncedIntList(this, nameof(CardsHeld));
    }

    // STUB: real Production Level is min(IND, POP[, RES]) off the Production track
    //  (§7.21), but that economy isn't modeled yet. Return a flat placeholder budget.
    // TODO: replace with the real IND/POP/RES calculation once the economy exists.
    public int GetProduction() => 6;
    public IEnumerable<Nation> GetControlledNations()
    {
        foreach (Nation nation in GameState.GetEntitiesOfType<Nation>())
        {
            if (nation.FactionId.Value == ID) yield return nation;
        } 
    }
}