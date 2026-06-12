using TT2026.NetworkedBoardGameEntitySystem;
using TT2026.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Entities;

public class BoardSpace : GameEntity
{

    public SyncedInt ControllerId;

    public BoardSpace() : base()
    {
        ControllerId = new SyncedInt(this, "ControllerId", defaultValue: -1, value: -1);
    }
}