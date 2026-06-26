using System;
using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class Faction : GameEntity
{
    public SyncedIntList Supply;

    public Faction()
    {
        Supply = new SyncedIntList(this, nameof(Supply));
    }
    public int GetProduction()
    {
        throw new NotImplementedException();
    }
    public IEnumerable<Nation> GetControlledNations()
    {
        foreach (Nation nation in GameState.GetEntitiesOfType<Nation>())
        {
            if (nation.FactionId.Value == ID) yield return nation;
        } 
    }
}