using System.Collections.Generic;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem;

public class TileOwnershipBehavior : GameBehavior
{
    public SyncedTileReferenceCollection TileOwnership;

    public TileOwnershipBehavior()
    {
        TileOwnership = new(this, nameof(TileOwnership));
    }

    public int GetOwnerOfTile(ICoordinate2d tileCoordinate)
    {
        return TileOwnership.Dict.GetKeyOfValue(tileCoordinate, fallback:-1);
    }

    public IEnumerable<ICoordinate2d> GetTilesOfOwner(int boardSpaceId)
    {
        return TileOwnership.Dict.GetValuesOfKey(boardSpaceId);
    }

    public void SetOwnerOfTile(ICoordinate2d tileCoordinate, int boardSpaceId)
    {
        TileOwnership.Dict.Set(boardSpaceId, tileCoordinate);
    }
}