using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem;

public class TileOwnershipBehavior : GameBehavior
{
    public SyncedEntityReferenceCollection TileOwnership;

    public TileOwnershipBehavior()
    {
        TileOwnership = new(this, "TileOwnership");
    }

    public int GetOwnerOfTile(int tileId)
    {
        return TileOwnership.Dict.GetKeyOfValue(tileId, fallback:-1);
    }

    public IEnumerable<int> GetTilesOfOwner(int tileId)
    {
        return TileOwnership.Dict.GetValuesOfKey(tileId);
    }

    public void SetOwnerOfTile(int tileId, int boardSpaceId)
    {
        TileOwnership.Dict.Set(boardSpaceId, tileId);
    }
}