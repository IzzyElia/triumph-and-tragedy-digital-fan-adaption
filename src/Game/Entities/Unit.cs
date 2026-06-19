using TT2026.Game.Definitions;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class Unit : GameEntity
{
    public SyncedInt UnitTypeId;
    public SyncedInt BoardSpaceId;
    public SyncedInt Pips;
    public SyncedInt NationId;

    public Unit()
    {
        UnitTypeId = new SyncedInt(this, nameof(UnitTypeId), -1);
        BoardSpaceId = new SyncedInt(this, nameof(BoardSpaceId), -1);
        NationId = new SyncedInt(this, nameof(NationId), -1);
        Pips = new SyncedInt(this, nameof(Pips), 0);
    }
        
    public UnitType UnitType => GameState.GetEntity<UnitType>(UnitTypeId.Value);
        
}